db.createCollection("key_ceremonies");
db.createCollection("elections");
db.createCollection("ballots");
db.createCollection("tallies");

db.key_ceremonies.createIndex({ KeyCeremonyId: 1, DataType: 1 });
db.key_ceremonies.createIndex({ KeyCeremonyId: 1, DesignatedId: 1, DataType: 1 });
db.key_ceremonies.createIndex({ KeyCeremonyId: 1, GuardianId: 1, DataType: 1 });
db.key_ceremonies.createIndex({ State: 1, DataType: 1 });

db.elections.createIndex({ ElectionId: 1, DataType: 1 });
db.elections.createIndex({ Name: 1, DataType: 1 });

db.ballots.createIndex({ ElectionId: 1, DataType: 1 });
db.ballots.createIndex({ UploadId: 1, DataType: 1 });
db.ballots.createIndex({ BallotCode: 1, DataType: 1 });
db.ballots.createIndex({ SerialNumber: 1, ElectionId: 1, DataType: 1 });

db.tallies.createIndex({ Name: 1, DataType: 1 });
db.tallies.createIndex({ ElectionId: 1, DataType: 1 });
db.tallies.createIndex({ TallyId: 1, DataType: 1 });
db.tallies.createIndex({ State: 1, DataType: 1 });
