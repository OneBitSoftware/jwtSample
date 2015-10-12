angular.module('MyApp').factory('dbService', function ($q) {
        return {
            init: function () {
                var deferred = $q.defer();

                var request = indexedDB.open("db");

                request.onupgradeneeded = function () {
                    // The database did not previously exist, so create object stores and indexes.
                    var db = request.result;

                    var usersStore = db.createObjectStore("users", { keyPath: "id" });
                    usersStore.createIndex("id_idx", "id", { unique: true });
                    usersStore.createIndex("token_idx", "token", { unique: false });

                    var syncStore = db.createObjectStore("syncs", { keyPath: "id" });
                    syncStore.createIndex("id_idx", "id", { unique: true });
                    syncStore.createIndex("created_idx", "created", { unique: false });
                    syncStore.createIndex("createdBy_idx", "createdBy", { unique: false });
                };

                request.onsuccess = function () {
                    deferred.resolve(request.result);
                };

                request.onerror = function () {
                    deferred.reject(request.error);
                };

                return deferred.promise;
            },
            add: function (storeName, entity) {
                var deferred = $q.defer();

                var add = function (db) {

                    var tx = db.transaction(storeName, "readwrite");
                    var store = tx.objectStore(storeName);

                    store.add(entity);

                    tx.oncomplete = function (e) {
                        // All requests have succeeded and the transaction has committed.
                        deferred.resolve(entity.id);
                    };

                    tx.onerror = function (r) {
                        deferred.reject(r.error);
                    };
                    tx.onabort = function (r) {
                        deferred.reject(r.error);
                    };
                }

                this.init().then(add);

                return deferred.promise;
            },
            put: function (storeName, entity) {
                var deferred = $q.defer();

                var put = function (db) {

                    var tx = db.transaction(storeName, "readwrite");
                    var store = tx.objectStore(storeName);

                    store.put(entity);

                    tx.oncomplete = function () {
                        // All requests have succeeded and the transaction has committed.
                        deferred.resolve(tx.result);
                    };

                    tx.onerror = function () {
                        deferred.reject(tx.error);
                    };
                    tx.onabort = function () {
                        deferred.reject(tx.error);
                    };
                }
                
                this.init().then(put);

                return deferred.promise;
            },
            getById: function (storeName, id) {
                var deferred = $q.defer();

                var getById = function (db) {

                    var tx = db.transaction(storeName, "readonly");
                    var store = tx.objectStore(storeName);
                    var index = store.index("id_idx");

                    var request = index.get(id);

                    request.onsuccess = function () {
                        deferred.resolve(request.result);
                    };

                    request.onerror = function () {
                        deferred.reject(request.error);
                    };
                    tx.onabort = function () {
                        deferred.reject(tx.error);
                    };
                }

                this.init().then(getById);

                return deferred.promise;
            },
            getByIndex: function (storeName, indexName, value) {
                var deferred = $q.defer();

                var getByIndex = function (db) {

                    var tx = db.transaction(storeName, "readonly");
                    var store = tx.objectStore(storeName);
                    var index = store.index(indexName);
                    var request = index.openCursor(IDBKeyRange.only(value)); //IDBKeyRange.only(value)
                    var results = [];
                    request.onsuccess = function () {
                        var cursor = request.result;
                        if (cursor) {
                            results.push(cursor.value);
                            cursor.continue();
                        } else {
                            deferred.resolve(results);
                        }
                    };
                    request.onerror = function (r) {
                        deferred.reject(r.error);
                    };
                    tx.onabort = function (r) {
                        deferred.reject(r.error);
                    };
                }

                this.init().then(getByIndex);

                return deferred.promise;
            },
            all: function (storeName, indexName) {
                var deferred = $q.defer();
                if (indexName === undefined) indexName = 'id_idx';

                var all = function (db) {
                    var tx = db.transaction(storeName, "readonly");
                    var store = tx.objectStore(storeName);
                    var index = store.index(indexName);

                    var request = index.openCursor(); //IDBKeyRange.only("Fred")
                    var results = [];
                    request.onsuccess = function () {
                        var cursor = request.result;
                        if (cursor) {
                            results.push(cursor.value);
                            cursor.continue();
                        } else {
                            deferred.resolve(results);
                        }
                    };
                    request.onerror = function () {
                        deferred.reject(request.error);
                    };
                    tx.onabort = function () {
                        deferred.reject(tx.error);
                    };
                }

                this.init().then(all);

                return deferred.promise;
            },
            count: function (storeName, indexName) {
                var deferred = $q.defer();
                if (indexName === undefined) indexName = 'id_idx';

                var count = function (db) {
                    var tx = db.transaction(storeName, "readonly");
                    var store = tx.objectStore(storeName);
                    var index = store.index(indexName);

                    var request = index.count();
                    request.onsuccess = function () {
                        deferred.resolve(request.result);
                    };
                    request.onerror = function () {
                        deferred.reject(request.error);
                    };
                    tx.onabort = function () {
                        deferred.reject(tx.error);
                    };
                }

                this.init().then(count);

                return deferred.promise;
            },
            remove: function () {
                //delete
            }
        };
    }
);


