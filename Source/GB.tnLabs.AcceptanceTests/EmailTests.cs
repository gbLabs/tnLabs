using System;
using System.Collections.Generic;
using GB.tnLabs.AcceptanceTests.Utils;
using GB.tnLabs.Core.Components;
using GB.tnLabs.Core.Repository;
using Xunit;

namespace GB.tnLabs.AcceptanceTests
{
    public class EmailTests
    {
        [RunnableInDebugOnlyFact]
        public void Send_Session_Email()
        {
            //arrange
            var session = CreateSession();
            //act 
            const bool expected = true;
            var actual = Email.BuildSessionEmails(session, "[AcceptanceTest]Test service").Send();            
            //assert
            Assert.Equal(expected, actual);
        }

        private Session CreateSession()
        {
            var session = new Session()
            {
                SessionId = 1,
                SessionName = "[AcceptanceTest]Test Session",
                StartDate = DateTimeOffset.Now,
                EndDate = DateTimeOffset.Now.AddHours(1),
                Lab = new Lab()
                {
                    LabId = 1,
                    Name = "[AcceptanceTest]Test Lab",
                    Description = "A test lab for all",
                    CreationDate = DateTime.Now.AddDays(-1),
                    Removed = false,
                    ImageName = "A_very_cool_image"
                },
                LabId = 1,
                Removed = false,
                SchedulingType = 1,
                Version = "Version 1.0",
                VmSize = "Small",
            };

            session.SessionUsers = CreateSessionUsers(session);
            session.VirtualMachines = CreateVirtualMachines(session);

            return session;
        }

        private ICollection<VirtualMachine> CreateVirtualMachines(Session session)
        {
            return new[]
            {
                new VirtualMachine()
                {
                    SessionId = 1,
                    Deleted = false,
                    Stopped = false,
                    User = new User()
                    {
                        FirstName = "An awesome user",
                        LastName = "With a cool name",
                        Email = "cromica@gmail.com",
                        Password = "oda"
                    },
                    Session = session,
                    VmAdminUser = "tnlabuser",
                    VmAdminPass = "tnlabpass",
                    VmName = "Awesome VM",
                    VmRdpPort = 6666,
                    VirtualMachineId = 1

                },
                new VirtualMachine()
                {
                    SessionId = 1,
                    Deleted = false,
                    Stopped = false,
                    Session = session,
                    User = new User()
                    {
                        FirstName = "An awesome user",
                        LastName = "With a cool name",
                        Email = "haiduc32@gmail.com",
                        Password = "oda"
                    },
                    VmAdminUser = "tnlabuser",
                    VmAdminPass = "tnlabpass",
                    VmName = "Awesome VM",
                    VmRdpPort = 6666,
                    VirtualMachineId = 1

                }
            };
        }

        private ICollection<SessionUser> CreateSessionUsers(Session session)
        {
            return new[]
            {
                new SessionUser()
                {
                    SessionId = 1,
                    SessionUserId = 1,
                    Session = session,
                    User = new User()
                    {
                        FirstName = "An awesome user",
                        LastName = "With a cool name",
                        Email = "cromica@gmail.com",
                        Password = "oda"
                    }
                },
                new SessionUser()
                {
                    SessionId = 1,
                    SessionUserId = 1,
                    Session = session,
                    User = new User()
                    {
                        FirstName = "An awesome user",
                        LastName = "With a cool name",
                        Email = "haiduc32@gmail.com",
                        Password = "oda"
                    }
                }
            };
        }
    }
}
