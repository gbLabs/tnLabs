(function () {
    'use strict';

    var app = angular.module('app');

    // Configure Toastr
    toastr.options.timeOut = 4000;
    toastr.options.positionClass = 'toast-bottom-right';

    // For use with the HotTowel-Angular-Breeze add-on that uses Breeze
    var remoteServiceName = 'breeze/trainings';

    var events = {
        controllerActivateSuccess: 'controller.activateSuccess',
        hasChangesChanged: 'datacontext.hasChangesChanged',
        spinnerToggle: 'spinner.toggle',
        screenResize: 'common.resize'
    };

    var config = {
        appErrorPrefix: '[HT Error] ', //Configure the exceptionHandler decorator
        docTitle: 'Training Labs: ',
        events: events,
        remoteServiceName: remoteServiceName,
        version: '2.0.0'
    };

    app.value('config', config);
    
    app.config(['$logProvider', function ($logProvider) {
        // turn debugging off/on (no info or warn)
        if ($logProvider.debugEnabled) {
            $logProvider.debugEnabled(true);
        }
    }]);
    
    //#region Configure the common services via commonConfig
    app.config(['commonConfigProvider', function (cfg) {
        cfg.config.controllerActivateSuccessEvent = config.events.controllerActivateSuccess;
        cfg.config.spinnerToggleEvent = config.events.spinnerToggle;
        cfg.config.screenResize = config.events.screenResize;
    }]);
    //#endregion
    
    //#region Configure the Breeze Validation Directive
    app.config(['zDirectivesConfigProvider', function (cfg) {
        cfg.zValidateTemplate =
                     '<span class="invalid"><i class="icon-warning-sign"></i>' +
                     'Field: %error%</span>';
        cfg.zRequiredTemplate =
            '';
    }]);
    
    app.filter('capitalize', function () {
        return function (input, scope) {
            if (input != null)
                input = input.toLowerCase();
            return input.substring(0, 1).toUpperCase() + input.substring(1);
        };
    });
})();