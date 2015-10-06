angular.module('MyApp', ['ngResource', 'ngMessages', 'ui.router', 'mgcrea.ngStrap', 'satellizer'])
  .config(function ($stateProvider, $urlRouterProvider, $authProvider) {
      $stateProvider
        .state('home', {
            url: '/',
            templateUrl: 'static/app/partials/home.html'
        })
        .state('login', {
            url: '/login',
            templateUrl: 'static/app/partials/login.html',
            controller: 'LoginCtrl'
        })
        .state('signup', {
            url: '/signup',
            templateUrl: 'static/app/partials/signup.html',
            controller: 'SignupCtrl'
        })
        .state('logout', {
            url: '/logout',
            template: null,
            controller: 'LogoutCtrl'
        })
        .state('profile', {
            url: '/profile',
            templateUrl: 'static/app/partials/profile.html',
            controller: 'ProfileCtrl',
            resolve: {
                authenticated: function ($q, $location, $auth) {
                    var deferred = $q.defer();

                    if (!$auth.isAuthenticated()) {
                        $location.path('/login');
                    } else {
                        deferred.resolve();
                    }

                    return deferred.promise;
                }
            }
        });

      $urlRouterProvider.otherwise('/');

      $authProvider.facebook({
          clientId: '1630314840562626',
          url: '/auth/facebook'
      });

      $authProvider.google({
          clientId: '519314902944-2u6u6o8trsmpa4kff1lr3h2hcaoanels.apps.googleusercontent.com',
          url: '/auth/google'
      });

      //$authProvider.github({
      //    clientId: '45ab07066fb6a805ed74'
      //});

      //$authProvider.linkedin({
      //    clientId: '77cw786yignpzj'
      //});

      //$authProvider.yahoo({
      //    clientId: 'dj0yJmk9SDVkM2RhNWJSc2ZBJmQ9WVdrOWIzVlFRMWxzTXpZbWNHbzlNQS0tJnM9Y29uc3VtZXJzZWNyZXQmeD0yYw--'
      //});

      //$authProvider.twitter({
      //    url: '/auth/twitter'
      //});

      //$authProvider.live({
      //    clientId: '000000004C12E68D'
      //});

      //$authProvider.oauth2({
      //    name: 'foursquare',
      //    url: '/auth/foursquare',
      //    clientId: 'MTCEJ3NGW2PNNB31WOSBFDSAD4MTHYVAZ1UKIULXZ2CVFC2K',
      //    redirectUri: window.location.origin || window.location.protocol + '//' + window.location.host,
      //    authorizationEndpoint: 'https://foursquare.com/oauth2/authenticate'
      //});
  });