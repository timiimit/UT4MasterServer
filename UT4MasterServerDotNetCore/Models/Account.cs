using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace UT4MasterServer.Models
{
    public class Account
    {
        //        {
        //	"id": "fd83abe496ca401497b5adf4e412bf2c",
        //	"displayName": "dc!",
        //	"name": "dc",
        //	"email": "email@email.email",
        //	"affiliationType": "Programmer",
        //	"failedLoginAttempts": 0,
        //	"lastLogin": "2022-12-14T23:39:48.417Z",
        //	"numberOfDisplayNameChanges": 0,
        //	"ageGroup": "UNKNOWN",
        //	"headless": false,
        //	"country": "CA",
        //	"lastName": "dc",
        //	"phoneNumber": "123",
        //	"preferredLanguage": "en",
        //	"canUpdateDisplayName": true,
        //	"tfaEnabled": true,
        //	"emailVerified": true,
        //	"minorVerified": false,
        //	"minorExpected": false,
        //	"minorStatus": "NOT_MINOR",
        //	"cabinedMode": false,
        //	"hasHashedEmail": false
        //}

        // TODO: Figure out what fields ^^^^ are actually needed in this model.

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string displayName { get; set; }
    }
}
