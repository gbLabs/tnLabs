var userIsTrainer, userIsOwner, parentRoles, userId, userContext, http;

(function () {
    'use strict';
    var controllerId = 'userdetail';
    angular.module('app').controller(controllerId, ['$scope', '$http', '$window', '$routeParams', 'common', 'config', 'datacontext', 'spinner', createUser]);

    function createUser($scope, $http, $window, $routeParams, common, config, datacontext,spinner) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.isNew = true;

        vm.errors = [];
        vm.identity = undefined;
        vm.saveChanges = saveChanges;
        vm.back = back;
        vm.addRole = addRole;

        activate();

        userContext = datacontext;
        http = $http;

        function activate() {
            onDestroy();
            common.activateController([initUser()], controllerId);
        }

        function back() {
            $window.history.back();
        }

        function initUser() {
            var val = $routeParams.id;
            vm.isNew = false;

            userId = val;

            datacontext.identity.getById(val)
                .then(function (data) {
                    vm.user = data;
                }, function (error) {
                    logError('Unable to get participant ' + val);
                    gotoUsers();
                });

            datacontext.getLoggedInUserRoles($http);
            datacontext.getUserRoles($http, userId);
            manageUserRoles();
        }

        function gotoUsers() {
            $location.path('/participants');
        }

        function saveChanges() {

            spinner.spinnerShow();
            //TODO: figure out why it's hardcoded!
            //vm.user.password = 'tnLabsP@ss1';
            //if (vm.user.entityAspect.validateEntity()) {
            //    datacontext.saveChanges().then(saveSucceded, saveFailed);
            //}
            
            function saveSucceded() {
                spinner.spinnerHide();
                $window.history.back();
            }

            function saveFailed() {
                spinner.spinnerHide();
            }
        }

        function onDestroy() {
            $scope.$on('$destroy', function () {
                datacontext.cancel();
            });
        }

         //Functions that handle the participant user role based on the subscription user role

        function manageUserRoles() {
            $('#add-new-roles').click(function () {
                $('#add-roles').removeClass('hidden');
                $.each(parentRoles, function (index, role) {
                    if (role == "Trainer") {
                        handleTrainerRole();
                    }
                    if (role == "Owner") {
                        handleOwnerRole();
                    }
                });
            });
        }

        function handleTrainerRole() {
            if (!userIsTrainer && !$("#available-roles:has(option[value='Trainer'])").length > 0) {
                $('#available-roles').append('<option value="Trainer">Trainer</option>');
            }
        }

        function handleOwnerRole() {
            if (!userIsOwner && !$("#available-roles:has(option[value='Owner'])").length > 0) {
                $('#available-roles').append('<option value="Owner">Owner</option>');
            }
        }

        function addRole() {
            var newRole = $('#available-roles').find(":selected").val();
            if (newRole != null && newRole != "") {
                datacontext.addUserRole($http, newRole, userId);
            }
        }
    }
})();



// Functions that handle the user role management for a subscription

function setParentRoles(data) {
    parentRoles = data;
    if (parentRoles.length == 1 && parentRoles[0] == "Member") {
        $('#add-new-roles').addClass('hidden');
    }
}

function setRole(data, newRole) {
    if (data) {
        if (newRole == "Trainer") {
            userIsTrainer = true;
        }
        else if (newRole == "Owner") {
            userIsOwner = true;
        }
        updateRoles(newRole);
    }
}

function updateUserRoles(roles)
{
    if (roles.length > 0) {
        $('#role-display').css("padding-top", "8px");
        $.each(roles, function (index, role) {
            setRole(true, role);
        });
    }
    else {
        $('#role-label').hide();
        $('#add-new-roles').hide();
    }
}

function updateRoles(role) {
    var element = '<span id=' + role + "_role" + '>' + role + '&nbsp;';
    if (parentRoles.length == 1 && parentRoles[0] == "Member") {
        element = element + '</span>&nbsp;&nbsp;';
    }
    else {
        element = element + '<a href="javascript:void(0)" onclick="removeUserRole(this)"><strong>X</strong></a></span>&nbsp;&nbsp;';
    }
    $('#user-roles').append(element);
}

function removeUserRole(removeLink) {
    var parent = $(removeLink).parent();
    if (parent != null) {
        var role = ($(parent).attr('id')).replace("_role", "");
        if (role != null && role != "") {
            userContext.removeUserRole(http, role, userId);
        }
    }
}

function removeRole(data, role) {
    if (data) {
        $('span[id^="' + role + '_role"]').remove();
    }
}
