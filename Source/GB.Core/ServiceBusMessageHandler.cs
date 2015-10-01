using Castle.Core.Logging;
using GB.tnLabs.Core.Annotations;
using GB.tnLabs.Core.Components;
using GB.tnLabs.Core.SBDtos;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GB.tnLabs.Core
{
	[UsedImplicitly]
	public class ServiceBusMessageHandler
	{
		private readonly string _queueName;

		private readonly ILogger _logger;
		private readonly Lazy<SignUp> _signUpLazy;
		private readonly Lazy<Sessions> _sessionsLazy;
		private readonly Lazy<Labs> _labsLazy;

		private QueueClient _client;

		protected SignUp SignUp { get { return _signUpLazy.Value; } }

		protected Sessions Sessions { get { return _sessionsLazy.Value; } }

		protected Labs Labs { get { return _labsLazy.Value; } }

		public ServiceBusMessageHandler(ILogger logger, Lazy<SignUp> signUpLazy, Lazy<Sessions> sessionsLazy, Lazy<Labs> labsLazy)
		{
			_logger = logger;
			_signUpLazy = signUpLazy;
			_sessionsLazy = sessionsLazy;
			_labsLazy = labsLazy;
			_queueName = CloudConfigurationManager.GetSetting("ServiceBusQueue");
		}

		public void Listen()
		{
			_logger.Info("Starting Service Bus Listener.");

			string connectionString =
				CloudConfigurationManager.GetSetting("ServiceBus");

			_client = QueueClient.CreateFromConnectionString(connectionString, _queueName);

			_client.OnMessage(MessageHandler, new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(5),
				MaxConcurrentCalls = 10
			});
		}

		public void Stop()
		{
			_logger.Info("Stopping Service Bus Listener.");
			_client.Close();
		}

		public async Task SendAsync(object payload, DateTime? scheduleUTC = null)
		{
			string connectionString = CloudConfigurationManager.GetSetting("ServiceBus");

			QueueClient client =
				QueueClient.CreateFromConnectionString(connectionString, _queueName);

			string jsonMessage = JsonConvert.SerializeObject(payload);
			BrokeredMessage message = new BrokeredMessage(jsonMessage);
			if (scheduleUTC.HasValue && scheduleUTC.Value > DateTime.UtcNow)
			{
				message.ScheduledEnqueueTimeUtc = scheduleUTC.Value;
			}

			message.Properties["Type"] = payload.GetType().Name;

			await client.SendAsync(message);
		}

		public void Send(object payload, DateTime? scheduleUTC = null)
		{
			string connectionString = CloudConfigurationManager.GetSetting("ServiceBus");

			QueueClient client =
				QueueClient.CreateFromConnectionString(connectionString, _queueName);

			string jsonMessage = JsonConvert.SerializeObject(payload);
			BrokeredMessage message = new BrokeredMessage(jsonMessage);
			if (scheduleUTC.HasValue && scheduleUTC.Value > DateTime.UtcNow)
			{
				message.ScheduledEnqueueTimeUtc = scheduleUTC.Value;
			}

			message.Properties["Type"] = payload.GetType().Name;

			client.Send(message);
		}

		public void SendSession(DateTimeOffset start, DateTimeOffset end, int sessionId, Guid version)
		{
			//TODO: errors at this level should be treated as critical
            DateTime utcActualStart = start.Subtract(Settings.SessionWarmUp).UtcDateTime;
            _logger.InfoFormat("Sending StartSessionRequest with SessionId: {0} Version: {1} Start(UTC): {2}",
                version, sessionId, utcActualStart);
			StartSessionRequest startRequest = new StartSessionRequest { Version = version, SessionId = sessionId};
            Send(startRequest, utcActualStart);

            DateTime utcActualEnd = end.Add(Settings.SessionCoolDown).UtcDateTime;
            _logger.InfoFormat("Sending EndSessionRequest with SessionId: {0} Version: {1} End(UTC): {2}",
                version, sessionId, utcActualEnd);
			EndSessionRequest endRequest = new EndSessionRequest { Version = version, SessionId = sessionId };
            Send(endRequest, utcActualEnd);
		}

		private void MessageHandler(BrokeredMessage message)
		{
			TimeSpan lockSpan = message.LockedUntilUtc - DateTime.UtcNow;
			lockSpan = lockSpan.Subtract(TimeSpan.FromSeconds(10));

			Timer lockUpdateTimer = new Timer(LockUpdateHandler, message, lockSpan, lockSpan);

			string messageType = null;
			//if there is an error before the execution gets to reading the message body, show in the exception
			//Unknown
			string jsonBody = "Unknown";

			try
			{
				messageType = (string)message.Properties["Type"];

				//find the Receive method that accepts the messageType type
				MethodInfo receiveMethod = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).SingleOrDefault(x =>
				{
					var parameters = x.GetParameters();
					if (parameters.Count() != 1) return false;
					return parameters[0].ParameterType.Name == messageType;
				});

				if (receiveMethod == null)
				{
					//TODO: not sure if we don't get an infinite loop after that
					_logger.Warn("Received message for which there is no handler. Type: " + messageType);
					_client.Abandon(message.LockToken);
					return;
				}

				Type messageTypeObject = receiveMethod.GetParameters()[0].ParameterType;

				jsonBody = message.GetBody<string>();
				object requestObject = JsonConvert.DeserializeObject(jsonBody, messageTypeObject);

				receiveMethod.Invoke(this, new[] { requestObject });
				
				_client.Complete(message.LockToken);
			}
			catch (Exception ex)
			{
						
				//TODO: if there is connection issue with the entity framework
				//we should put the message back on the queue, and let another service 
				//handle it
				_client.DeadLetter(message.LockToken);
				
				string errorMessage = string.Format(
					"Failed processing the message of type [{0}] with payload:\r\n{1}",
					messageType, jsonBody);
				_logger.Fatal(errorMessage, ex);
			}
			finally
			{
				//hope this is enought to stop the timer
				lockUpdateTimer.Dispose();
			}
		}

		private void LockUpdateHandler(object state)
		{
			BrokeredMessage message = (BrokeredMessage)state;
			try
			{
				message.RenewLock();
			}
			catch (Exception ex)
			{
				_logger.Warn("Could not renew lock on a message.", ex);
			}
			
		}

		[UsedImplicitly]
		private void Receive(AssignCertificateRequest request)
		{
			SignUp.AssignCertificate(request.SubscriptionId, request.IdentityId);
		}

		[UsedImplicitly]
		private void Receive(StartSessionRequest request)
		{
			Sessions.Start(request);
		}

		[UsedImplicitly]
		private void Receive(EndSessionRequest request)
		{
			Sessions.End(request);
		}

		[UsedImplicitly]
		private void Receive(VMReadyForCaptureRequest request)
		{
			Labs.VMReadyForCapture(request);
		}

		[UsedImplicitly]
		private void Receive(CreateBaseVMImageRequest request)
		{
			Labs.CreateBaseVMImage(request);
		}
	}
}
