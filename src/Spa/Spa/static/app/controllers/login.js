angular.module('MyApp')
  .controller('LoginCtrl', function ($scope, $alert, $auth, userService) {

      var successMessage = function () {
          userService.getCurrentUser();
          console.log('success');
      }

      var errorMessage = function (error) {
          if (error.status === 401) {
              console.log('error 401');
          } else {
              console.log('error');
          }
      }

    $scope.login = function () {
      $auth.login({ email: $scope.email, password: $scope.password }).then(successMessage, errorMessage);
    };

    $scope.authenticate = function(provider) {
      $auth.authenticate(provider).then(successMessage, errorMessage);
    };

  });