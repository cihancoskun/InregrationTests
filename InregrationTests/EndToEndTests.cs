using EasyNetQ;
using InregrationTests.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Data;
using System.Data.SqlClient;

namespace InregrationTests
{
    [TestFixture]
    public class EndToEndTests
    {
        string connectionString = "host=localhost:5672;username=mert;password=abuzer";

        [Test]
        public void user_register_end_to_end()
        {
            var userId = "Id";
            var email = "test@test.com";

            using (var bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.Send("USER_CREATED", JsonConvert.SerializeObject(new
                {
                    Id = userId,
                    Email = email,
                    FirstName = "firstname",
                    LastName = "lastname",
                    Password = "password",
                    IsDirty = true
                }));
            }

            System.Threading.Thread.Sleep(1000);

            // CRMService Listener açık olmalı
            // Kuyruğa atılan veriyi okuyup db'ye user'ı atıp atmadığını kontrol edeceğiz.

            var dataTable = new DataTable();
            var count = 0;

            using (var connection = new SqlConnection("connectionString"))
            {
                string query = @"SELECT * FROM USER WHERE ID = @ID";

                using (SqlCommand sqlCommand = new SqlCommand(query, connection))
                {
                    sqlCommand.Parameters.AddWithValue("@ID", userId);

                    connection.Open();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                    {
                        adapter.Fill(dataTable);
                    }

                    while (dataTable.Rows == null || dataTable.Rows.Count < 1 && count < 5)
                    {
                        System.Threading.Thread.Sleep(5000);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                        {
                            adapter.Fill(dataTable);
                        }
                        count++;
                    }

                    connection.Close();
                }
            }

            if (count == 5)
            {
                Assert.Fail("Sql'den 5 kere veriyi okumaya çalışıldı, sonuç alınamadı.");
            }
            else
            {
                var returnEmail = dataTable.Rows[0]["Email"].ToString();

                Assert.AreEqual(email, returnEmail);

                var client = new MongoClient("");
                var MongoDatabase = client.GetDatabase("");

                var _collection = MongoDatabase.GetCollection<User>("Users");
                var filter = Builders<User>.Filter.Eq("_id", userId);
                var mongoUser = _collection.Find(filter).Single();

                Assert.AreEqual(mongoUser.Email, email);
            }
        }
    }
}
