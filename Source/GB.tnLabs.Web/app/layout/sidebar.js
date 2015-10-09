(function () {
    'use strict';

    var controllerId = 'sidebar';
    angular.module('app').controller(controllerId,
        ['$route', 'config', 'routes', sidebar]);

    function sidebar($route, config, routes) {
        var vm = this;

        vm.isCurrent = isCurrent;

        activate();

        function activate() { getNavRoutes(); }

        function getNavRoutes() {
            var isOwner = $('#is-owner').val();
            var indexOfUsersRoute = -1;

            if (isOwner == "False") {
                $.each(routes, function (index, route) {
                    if (route.url == "/users") {
                        indexOfUsersRoute = index;
                    }
                })
            }

            if (indexOfUsersRoute != -1)
                routes.splice(indexOfUsersRoute, 1);

            vm.navRoutes = routes.filter(function(r) {
                return r.config.settings && r.config.settings.nav;
            }).sort(function(r1, r2) {
                return r1.config.settings.nav - r2.config.settings.nav;
            });
        }

        function isCurrent(route) {
            if (!route.config.title || !$route.current || !$route.current.title) {
                return '';
            }
            var menuName = route.config.title;
            return $route.current.title.substr(0, menuName.length) === menuName ? 'active' : '';
        }
    };
})();
