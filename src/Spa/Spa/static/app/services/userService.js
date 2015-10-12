angular.module('MyApp').factory('userService',
    function ($http, dbService, $q) {
        return {
            getCurrentUser: function () {
                var deferred = $q.defer();

                var token = localStorage.satellizer_token;
                if (!token) {
                    deferred.reject('satellizer_token is missing');
                    return deferred.promise;
                }
                dbService.getByIndex('users', 'token_idx', token).then(
                    function (users) {
                        if (users.length !== 0) {
                            deferred.resolve(users[0]);
                        } else {
                            $http.get('/api/me').then(
                               function (response) {
                                   if (response.data.id !== undefined) {
                                       var user = response.data;
                                       user.token = token;

                                       dbService.getById('users', user.id).then(
                                           function (existingUser) {

                                               if (existingUser !== undefined) {

                                                   dbService.put('users', user).then(
                                                       function (result) {
                                                           deferred.resolve(user);
                                                       },
                                                       function (error) {
                                                           deferred.reject(error);
                                                       }
                                                    );

                                               } else {

                                                   dbService.add('users', user).then(
                                                       function (result) {
                                                           deferred.resolve(user);
                                                       },
                                                       function (error) {
                                                           deferred.reject(error);
                                                       }
                                                    );
                                               };
                                           },
                                           function (error) {
                                               deferred.reject(error);
                                           }
                                       );
                                   }
                                   else
                                   {
                                       deferred.reject();
                                   }
                               },
                               function (error) {
                                   deferred.reject(error);
                               }
                            );
                        }
                    },
                    function (error) {
                        deferred.reject(error);
                    }
                );

                return deferred.promise;
            }
        };
    });


