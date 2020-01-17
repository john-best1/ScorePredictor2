using Newtonsoft.Json.Linq;
using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ScorePredictor.Data
{
    public class TeamDatabase
    {
        SqlConnectionStringBuilder builder;
        HttpClient client = new HttpClient();

        public TeamDatabase()
        {
            builder = new SqlConnectionStringBuilder();
            builder.DataSource = "scorepredictordb.database.windows.net";
            builder.UserID = "jbest";
            builder.Password = "databasepassword*1";
            builder.InitialCatalog = "scorepredictordb";

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");
        }

        public async Task<Team> addTeamToDatabase(int id, int leagueId)
        {
            Team team = new Team();
            team.TeamId = id;
            team.LeagueId = leagueId;
            JObject teamJObject = await getTeamDataFromApi(id);
            team.LongName = teamJObject["name"].ToString();
            team.ShortName = teamJObject["shortName"].ToString();
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("insert into Team values(" +
                "@longname,@shortname, @teamid, @leagueid)", connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@longname", team.LongName);
                    cmd.Parameters.AddWithValue("@shortname", team.ShortName);
                    cmd.Parameters.AddWithValue("@teamid", team.TeamId);
                    cmd.Parameters.AddWithValue("@leagueid", team.LeagueId);
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return team;
        }

        public async Task<Team> getTeamFromDatabase(int id, int leagueId)
        {
            Team team = new Team();
            team.TeamId = id;
            if (databaseCheck(id)){
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand("SELECT * from Team as team " +
                            "Where team.TeamId = " + id, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            reader.Read();
                            team.LongName = reader["LongName"].ToString();
                            team.ShortName = reader["ShortName"].ToString();
                            reader.Close();
                        }
                        connection.Close();
                    }
                }
                return team;
            }
            else
            {
                return await addTeamToDatabase(id, leagueId);
            }
        }

        public async Task<Dictionary<int,string>> getTeamsFromDatabase(int leagueCode, JArray standings)
        {
            Dictionary<int, string> teams = new Dictionary<int, string>();
            JArray teamStandings = (JArray)standings[0]["table"];
            int totalTeams = teamStandings.Count;
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("SELECT * from Team as team " +
                        "Where team.LeagueId = " + leagueCode, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read()){
                            teams.Add(int.Parse(reader["TeamId"].ToString()), reader["shortName"].ToString());
                        }
                    reader.Close();
                    }
                    connection.Close();
                }
            }
            while (teams.Count < totalTeams)
            {
                foreach (JObject row in teamStandings)
                {
                    if (!teams.ContainsKey(int.Parse(row["team"]["id"].ToString())))
                    {
                        Team team = await addTeamToDatabase(int.Parse(row["team"]["id"].ToString()), leagueCode);
                        teams.Add(team.TeamId, team.ShortName);
                    }
                }
            }
            return teams;
        }

        public bool databaseCheck(int id)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {

                using (SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) from Team as team " +
                        "Where team.TeamId = " + id, connection))
                {
                    connection.Open();
                    int teamCount = (int)sqlCommand.ExecuteScalar();
                    connection.Close();
                    if (teamCount > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<JObject> getTeamDataFromApi(int id)
        {
            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/teams/" + id))
            {
                if (response.IsSuccessStatusCode)
                {
                    return JObject.Parse(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
