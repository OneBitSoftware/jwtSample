angular.module('MyApp').factory('syncService', function(dbService) {
        return {
            syncAll: function() {

                //get last sync period
                //decide if to sync
                //is it manual sync

                //filter the data for user to sync according the modified date
                //send the data
                //recieve new sync record


                //dbService.all('users').then(function(users) {
                //    dbService.all('games').then(function(games) {
                //        dbService.all('results').then(function(results) {
                //            var syncData = 
                //        });
                //    });
                //});
            }
        };
    }
);


