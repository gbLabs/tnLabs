(function () {
    'use strict';

    var app = angular.module('app');

    // Collect the routes
    app.constant('routes', getRoutes());

    // Configure the routes and route resolvers
    app.config(['$routeProvider', 'routes', routeConfigurator]);
    function routeConfigurator($routeProvider, routes) {

        routes.forEach(function (r) {
            //$routeProvider.when(r.url, r.config);
            setRoute(r.url, r.config);
        });
        $routeProvider.otherwise({ redirectTo: '/' });

        function setRoute(url, definition) {
            // Sets resolvers for all of the routes
            // by extending any existing resolvers (or creating a new one).
            definition.resolve = angular.extend(definition.resolve || {}, {
                prime: prime
            });
            $routeProvider.when(url, definition);
            return $routeProvider;
        }
    }

    prime.$inject = ['datacontext'];

    function prime(dc) {
        return dc.prime();
    }

    // Define the routes 
    function getRoutes() {
        return [
            {
                url: '/',
                config: {
                    templateUrl: 'app/dashboard/dashboard.html',
                    title: 'dashboard',
                    settings: {
                        nav: 1,
                        content: '<i class="fa fa-dashboard"></i><span class="hidden-sm"> Dashboard</span>'
                    }
                }
            }, {
                url: '/labs',
                config: {
                    title: 'Labs',
                    templateUrl: 'app/lab/labs.html',
                    settings: {
                        nav: 2,
                        content: '<i class="fa fa-laptop"></i><span class="hidden-sm"> Labs</span>'
                    }
                }
            }, {
                url: '/sessions',
                config: {
                    title: 'Sessions',
                    templateUrl: 'app/session/sessions.html',
                    settings: {
                        nav: 3,
                        content: '<i class="fa fa-calendar"></i> <span class="hidden-sm"> Sessions</span>'
                    }
                }
            }, {
                url: '/users',
                config: {
                    title: 'Users',
                    templateUrl: 'app/user/users.html',
                    settings: {
                        nav: 5,
                        content: '<i class="fa fa-users"></i> <span class="hidden-sm"> Users</span>'
                    }
                }
            }, {
                url: '/session/:id',
                config: {
                    title: 'Session',
                    templateUrl: 'app/session/sessiondetail.html'
                }
            }
            , {
                url: '/lab/:id',
                config: {
                    title: 'Lab detail',
                    templateUrl: 'app/lab/labdetail.html'
                }
            }
            , {
                url: '/participant/:id',
                config: {
                    title: 'Participant detail',
                    templateUrl: 'app/user/userdetail.html'
            }
        }
        ];
    }
})();