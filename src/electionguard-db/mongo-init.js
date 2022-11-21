db.createCollection("key_ceremonies");
db.key_ceremonies.createIndex({ completed_at: 1 });
db.key_ceremonies.createIndex({ key_ceremony_name: 1 });
