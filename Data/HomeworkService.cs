using System.Collections.Generic;
using retns.api.Data.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace retns.api.Data {
    public class HomeworkService {
        private readonly IMongoCollection<HomeworkWeek> _homeworkWeeks;

        public HomeworkService(IConfiguration config) {
            var client = new MongoClient(config.GetConnectionString("HomeworkDb"));
            var database = client.GetDatabase("retns");
            _homeworkWeeks = database.GetCollection<HomeworkWeek>("Homework");
        }

        public async Task<List<HomeworkWeek>> Get() {
            return (await _homeworkWeeks.FindAsync(homework => true)).ToList();
        }

        public async Task<HomeworkWeek> Get(string id) {
            return (await _homeworkWeeks.FindAsync<HomeworkWeek>(homework => homework.Id == id)).FirstOrDefault();
        }

        public async Task<HomeworkWeek> Create(HomeworkWeek homework) {
            await _homeworkWeeks.InsertOneAsync(homework);
            return homework;
        }

        public async Task Update(string id, HomeworkWeek homeworkIn) {
            await _homeworkWeeks.ReplaceOneAsync(homework => homework.Id == id, homeworkIn);
        }

        public async Task Remove(HomeworkWeek homeworkIn) {
            await _homeworkWeeks.DeleteOneAsync(homework => homework.Id == homeworkIn.Id);
        }

        public async Task Remove(string id) {
            await _homeworkWeeks.DeleteOneAsync(homework => homework.Id == id);
        }
    }
}