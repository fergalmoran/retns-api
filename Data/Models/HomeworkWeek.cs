using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace retns.api.Data.Models {
    public class HomeworkWeek {
        public HomeworkWeek(string id) {
            this.Id = id;
            this.Days = new Dictionary<DayOfWeek, Dictionary<string, string>>();
        }
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement("WeekCommencing")]
        public DateTime WeekCommencing { get; set; }

        [BsonElement("WeekCNotesommencing")]
        public string Notes { get; set; }

        [BsonElement("Subjects")]
        public List<string> Subjects;
        
        [BsonElement("Days")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<DayOfWeek, Dictionary<string, string>> Days { get; set; }
    }
}
