(function () {
    'use strict';

    var serviceId = 'model.validation';

    angular.module('app').factory(serviceId, ['common', modelValidation]);

    function modelValidation(common) {
        var entityNames;
        var log = common.logger.getLogFn(serviceId);
        var Validator = breeze.Validator,
            requireReferenceValidator,
            requireDateReferenceValidator,
            emailValidator,
            endTimeValidator;

        var service = {
            applyValidators: applyValidators,
            createAndRegister: createAndRegister
        };

        return service;

        function applyValidators(metadataStore) {
            applyLabRequireReferenceValidators(metadataStore);
            applySessionRequireReferenceValidators(metadataStore);
            applyUserRequireReferenceValidators(metadataStore);
            log('Validators applied', null, serviceId);
        }

        function applyUserRequireReferenceValidators(metadataStore) {
            var navigations = ['email', 'firstName', 'lastName'];
            var entityType = metadataStore.getEntityType(entityNames.user);

            navigations.forEach(function (propertyName) {
                entityType.getProperty(propertyName).validators
                    .push(requireReferenceValidator);

            });

            //apply email validator
            entityType.getProperty('email').validators
                    .push(emailValidator);
        }

        function applyLabRequireReferenceValidators(metadataStore) {
            var navigations = ['description'];
            var entityType = metadataStore.getEntityType(entityNames.lab);

            navigations.forEach(function (propertyName) {
                entityType.getProperty(propertyName).validators
                    .push(requireReferenceValidator);
            });

        }

        function applySessionRequireReferenceValidators(metadataStore) {
            var navigations = ['lab'];
            var entityType = metadataStore.getEntityType(entityNames.session);

            navigations.forEach(function (propertyName) {
                var property = entityType.getProperty(propertyName);
                property.validators
                    .push(requireReferenceValidator);
            });

            entityType.getProperty('startDate').validators
                .push(requireDateReferenceValidator);
        }

        function createAndRegister(eNames) {
            entityNames = eNames;
            requireReferenceValidator = createRequireReferenceValidator();
            requireDateReferenceValidator = createDateRequireReferenceValidator();
            endTimeValidator = createEndTimeValidator();
            emailValidator = createEmailValidator();
            //// Step 2) Tell breeze about it
            Validator.register(requireReferenceValidator);
            Validator.register(requireDateReferenceValidator);
            log('Validators created and registered', null, serviceId, false);
        }

        function createEmailValidator() {
            return breeze.Validator.makeRegExpValidator(
                            "emailAddress",
                            /^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$/,
                            "The %displayName% is not a valid email address");
        }

        function createRequireReferenceValidator() {
            var name = 'requireReferenceEntity';
            // isRequired = true so zValidate directive displays required indicator
            var ctx = { messageTemplate: 'Missing %displayName%', isRequired: true };
            var val = new Validator(name, valFunction, ctx);
            return val;

            // passes if reference has a value and is not the nullo (whose id===0)
            function valFunction(value) {
                return value ? value.id !== 0 : false;
            }
        }

        function createDateRequireReferenceValidator() {
            var name = 'requireDateReferenceEntity';
            // isRequired = true so zValidate directive displays required indicator
            var ctx = { messageTemplate: 'Missing %displayName%', isRequired: true };
            var val = new Validator(name, valFunction, ctx);
            return val;

            // passes if reference has a value and is not the nullo (whose id===0)
            function valFunction(value, context) {

                var mValue = moment(value);
                var mDefaultValue = moment(new Date(1900, 0, 1));
                return value ? !mValue.isSame(mDefaultValue) : false;
            }
        }

        function createEndTimeValidator() {
            var name = 'endTimeValidator';
            // isRequired = true so zValidate directive displays required indicator
            var ctx = { messageTemplate: 'Must be greater than start time', isRequired: false };
            var val = new Validator(name, valFunction, ctx);
            return val;

            // passes if reference has a value and is not the nullo (whose id===0)
            function valFunction(value, context) {
                var endTime = moment(value, 'hh:mm');
                var startTime = moment(context.entity.startTime, 'hh:mm');
                if (!endTime.isValid()) return false;
                if (!startTime.isValid()) return false;
                if (startTime.isAfter(endTime) || startTime.isSame(endTime)) return false;
                return true;
            }
        }
    }
})();