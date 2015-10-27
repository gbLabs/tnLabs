var parentRoles, userId, userContext, http;

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
                    vm.identity = data;
                }, function (error) {
                    logError('Unable to get participant ' + val);
                    gotoUsers();
                });

            datacontext.getLoggedInUserRoles($http);
            datacontext.getUserRoles($http, userId);
        }

        function gotoUsers() {
            $location.path('/participants');
        }

        function onDestroy() {
            $scope.$on('$destroy', function () {
                datacontext.cancel();
            });
        }
    }
})();



// Functions that handle the user role management for a subscription

function setParentRoles(data) {
    parentRoles = data;
}

function setRole(success, removeOption, newRole) {
    if (success) {
        if (removeOption) {
            $("#available-roles option[value='" + newRole + "']").remove();
        }
        updateRoles(newRole);
    }
}

function updateUserRoles(roles) {
    $('#role-display').css("padding-top", "8px");
    $.each(roles, function (index, role) {
        setRole(true, false, role);
    });
}

function updateRoles(role) {
    var element = '<span id=' + role + "_role" + '>' + role;
    if (role == "Member") {
        element = element + '</span>&nbsp;&nbsp;';
    }
    else {
        element = element + '&nbsp;<a href="javascript:void(0)" onclick="removeUserRole(this)"><strong>X</strong></a></span>&nbsp;&nbsp;';
    }
    $('#user-roles').append(element);
    manageUserRoles();
}

function manageUserRoles() {
    $('#available-roles').css("padding-top", "8px");
    if (parentRoles != null) {
        $.each(parentRoles, function (index, role) {
            if (role == "Trainer" && $("#available-roles option[value=" + role + "]").length == 0 && $('span[id^="' + role + '_role"]').length == 0) {
                $('#available-roles').append('<option value="Trainer">Trainer</option>');
            }
            if (role == "Owner" && $("#available-roles option[value=" + role + "]").length == 0 && $('span[id^="' + role + '_role"]').length == 0) {
                $('#available-roles').append('<option value="Owner">Owner</option>');
            }
        });
    }
}

function addRole() {
    var newRole = $('#available-roles').find(":selected").val();
    if (newRole != null && newRole != "" && $('span[id^="'+ newRole + '_role"]').length == 0) {
        userContext.addUserRole(http, newRole, userId);
    }
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
        if (parentRoles.indexOf(role) > -1 && $("#available-roles option[value=" +role + "]").length == 0) {
            $('#available-roles').append('<option value="' + role + '">' + role + '</option>');
        }
    }
}
