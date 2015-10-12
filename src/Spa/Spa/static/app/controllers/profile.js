angular.module('MyApp')
  .controller('ProfileCtrl', function ($scope, $rootScope, dbService, userService, utilsService) {

      var error = function (e) {
          alert('profile error');
          console.log('profile error: '+e);
      };

      $scope.getProfile = function () {
          userService.getCurrentUser().then(function (user) {
              $scope.user = user;
          }, error);
      };

      $scope.updateProfile = function () {
          $scope.user.modified = utilsService.utcNow();
          dbService.put('users', $scope.user).then(function () {
              $rootScope.user = $scope.user;
              alert('profile update success');
          }, error);
      };
      $scope.getProfile();
  });