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
    public class MatchDatabase
    {
        LeagueService leagueService = new LeagueService();
        HttpClient client = new HttpClient();
        int season = 2019;   //change this every season

        // returns a match object, either from databse or API
        public async Task<Match> getMatch(Match match)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "scorepredictordb.database.windows.net";
            builder.UserID = "jbest";
            builder.Password = "databasepassword*1";
            builder.InitialCatalog = "scorepredictordb";

            if (databaseCheck(builder, match))
            {
                //get from database and return
                match = getMatchFromDatabase(builder, match);
            }
            else
            {
                match = await getMatchFromApi(builder, match);
            }


            return match;
        }


        // match hasn't been generated before so isn't in database, get it from api instead
        private async Task<Match> getMatchFromApi(SqlConnectionStringBuilder builder, Match match)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");
            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/matches/" + match.MatchId))
            {
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());

                    if (jsonObject["match"]["status"].ToString() == "FINISHED")
                    {
                        match = populateFinishedMatch(jsonObject, match);
                    }
                    else
                    {
                        match = populateFutureMatch(jsonObject, builder, match);

                        match.HomeStats = await leagueService.getStats(match.LeagueId, match.HomeTeamId, match.MatchId);
                        match.AwayStats = await leagueService.getStats(match.LeagueId, match.AwayTeamId, match.MatchId, false);

                        MatchUtilities.getWDLString(match.HomeStats);
                        MatchUtilities.getWDLString(match.AwayStats);
                        match = await getRecentFormFromApi(match);
                    }
                    addMatchToDatabase(builder, match);
                    return match;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }


        // first time match is generated or first time since match has finished. Add it to database
        private void addMatchToDatabase(SqlConnectionStringBuilder builder, Match match)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("insert into Match values(" +
                "@matchid,@hometeamid,@hometeamname,@awayteamid,@awayteamname,@utcdate,@leaguename,@stadium," +
                "@leagueid,@imageurl,@predictedresult,@predictionstring,@predictedhomescore,@predictedawayscore," +
                "@finished,@predictionmade,@resultretrieved, @homegoalsscored, @awaygoalsscored, @resultpredictionmade);", connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@matchid", match.MatchId);
                    cmd.Parameters.AddWithValue("@hometeamid", match.HomeTeamId);
                    cmd.Parameters.AddWithValue("@hometeamname", match.HomeTeamName);
                    cmd.Parameters.AddWithValue("@awayteamid", match.AwayTeamId);
                    cmd.Parameters.AddWithValue("@awayteamname", match.AwayTeamName);
                    cmd.Parameters.AddWithValue("@utcdate", match.UtcDate);
                    cmd.Parameters.AddWithValue("@leaguename", match.LeagueName);
                    cmd.Parameters.AddWithValue("@stadium", match.Stadium);
                    cmd.Parameters.AddWithValue("@leagueid", match.LeagueId);
                    cmd.Parameters.AddWithValue("@imageurl", match.ImageUrl);
                    cmd.Parameters.AddWithValue("@predictedresult", match.predictedResult);
                    cmd.Parameters.AddWithValue("@predictionstring", match.predictionString);
                    cmd.Parameters.AddWithValue("@predictedhomescore", match.predictedScore[0]);
                    cmd.Parameters.AddWithValue("@predictedawayscore", match.predictedScore[1]);
                    cmd.Parameters.AddWithValue("@finished", match.finished);
                    cmd.Parameters.AddWithValue("@predictionmade", match.predictionMade);
                    cmd.Parameters.AddWithValue("@resultretrieved", match.resultRetrieved);
                    cmd.Parameters.AddWithValue("@homegoalsscored", match.homeGoals);
                    cmd.Parameters.AddWithValue("@awaygoalsscored", match.awayGoals);
                    if (!match.finished && !match.predictionResultRecorded && match.predictionMade)
                    {
                        cmd.Parameters.AddWithValue("@resultpredictionmade", 1);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@resultpredictionmade", 0);
                    }

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            if (!match.finished)
            {
                addStatsToDatabase(builder, match.HomeStats, match.HomeTeamId, match.MatchId);
                addStatsToDatabase(builder, match.AwayStats, match.AwayTeamId, match.MatchId);
            }
            else
            {
                addGoalScorersToDatabase(builder, match.homeGoalScorers, match.HomeTeamId, match.MatchId);
                addGoalScorersToDatabase(builder, match.awayGoalScorers, match.AwayTeamId, match.MatchId);
                if (!match.predictionResultRecorded && match.predictionMade)
                {
                    //increment predictions count
                }
            }
        }



        private void addStatsToDatabase(SqlConnectionStringBuilder builder, MatchStats stats, int teamId, string matchId)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("insert into MatchStats values(" +
                "@position,@homeorawayposition,@name,@matchesplayed,@won,@drawn,@lost,@homeorawaymatchesplayed," +
                "@homeorawaywon,@homeorawaydrawn,@homeorawaylost,@goalsfor,@goalsagainst,@goaldifference," +
                "@points,@goalsscoredpergame,@goalsconcededpergame, @wdl, @homeorawaywdl, @overallformstring, @homeorawayformstring," +
                "@predictionpoints, @overalllast6scored, @overalllast6conceded, @homeorawaylast6scored, @homeorawaylast6conceded," +
                "@teamid, @matchid);", connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@position", stats.position);
                    cmd.Parameters.AddWithValue("@homeorawayposition", stats.HomeOrAwayPosition);
                    cmd.Parameters.AddWithValue("@name", stats.name);
                    cmd.Parameters.AddWithValue("@matchesplayed", stats.matchesPlayed);
                    cmd.Parameters.AddWithValue("@won", stats.won);
                    cmd.Parameters.AddWithValue("@drawn", stats.drawn);
                    cmd.Parameters.AddWithValue("@lost", stats.lost);
                    cmd.Parameters.AddWithValue("@homeorawaymatchesplayed", stats.homeOrAwayMatchesPlayed);
                    cmd.Parameters.AddWithValue("@homeorawaywon", stats.homeOrAwayWon);
                    cmd.Parameters.AddWithValue("@homeorawaydrawn", stats.homeOrAwayDrawn);
                    cmd.Parameters.AddWithValue("@homeorawaylost", stats.homeOrAwayLost);
                    cmd.Parameters.AddWithValue("@goalsfor", stats.goalsFor);
                    cmd.Parameters.AddWithValue("@goalsagainst", stats.goalsAgainst);
                    cmd.Parameters.AddWithValue("@goaldifference", stats.goalDifference);
                    cmd.Parameters.AddWithValue("@points", stats.points);
                    cmd.Parameters.AddWithValue("@goalsscoredpergame", stats.goalsScoredPerGame);
                    cmd.Parameters.AddWithValue("@goalsconcededpergame", stats.goalsConcededPerGame);
                    cmd.Parameters.AddWithValue("@wdl", stats.WDL);
                    cmd.Parameters.AddWithValue("@homeorawaywdl", stats.homeOrAwayWDL);
                    cmd.Parameters.AddWithValue("@overallformstring", stats.overallFormString);
                    cmd.Parameters.AddWithValue("@homeorawayformstring", stats.homeOrAwayFormString);
                    cmd.Parameters.AddWithValue("@predictionpoints", stats.PredictionPoints);
                    cmd.Parameters.AddWithValue("@overalllast6scored", stats.overallLast6Scored);
                    cmd.Parameters.AddWithValue("@overalllast6conceded", stats.overallLast6Conceded);
                    cmd.Parameters.AddWithValue("@homeorawaylast6scored", stats.homeOrAwayLast6Scored);
                    cmd.Parameters.AddWithValue("@homeorawaylast6conceded", stats.homeOrAwayLast6Conceded);
                    cmd.Parameters.AddWithValue("@teamid", teamId);
                    cmd.Parameters.AddWithValue("@matchid", matchId);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        // add goalscorers for a finished match to database
        private void addGoalScorersToDatabase(SqlConnectionStringBuilder builder, List<Goal> goalScorers, int teamId, string matchId)
        {
            if (goalScorers.Any())
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    foreach (Goal goal in goalScorers)
                    {
                        using (SqlCommand cmd = new SqlCommand("insert into Goal values(" +
                            "@minute, @scorer, @assist, @matchid, @teamid);", connection))
                        {
                            cmd.Parameters.AddWithValue("@minute", goal.minute);
                            cmd.Parameters.AddWithValue("@scorer", goal.scorer);
                            cmd.Parameters.AddWithValue("@assist", goal.assist);
                            cmd.Parameters.AddWithValue("@matchid", matchId);
                            cmd.Parameters.AddWithValue("@teamid", teamId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
            }
        }


        public async Task<Match> getRecentFormFromApi(Match match)
        {
            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/competitions/" + match.LeagueId + "/matches?status=FINISHED&season=" + season))
            {
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    JArray allCompetitionMatches = (JArray)jsonObject["matches"];
                    match.HomeStats = MatchUtilities.getTeamForm(match.HomeStats, match.HomeTeamId, allCompetitionMatches);
                    match.AwayStats = MatchUtilities.getTeamForm(match.AwayStats, match.AwayTeamId, allCompetitionMatches, false);
                    match.HomeStats.overallFormString = MatchUtilities.getWDLString(match.HomeStats);
                    match.AwayStats.overallFormString = MatchUtilities.getWDLString(match.AwayStats);
                    Predictor.generatePrediction(match);
                    return match;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        private bool databaseCheck(SqlConnectionStringBuilder builder, Match match)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {

                using (SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) from Match as match " +
                        "Where match.MatchId = " + match.MatchId + "; ", connection))
                {
                    connection.Open();
                    int userCount = (int)sqlCommand.ExecuteScalar();
                    connection.Close();
                    if (userCount > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Match populateFutureMatch(JObject jsonObject, SqlConnectionStringBuilder builder, Match match)
        {
            match.Stadium = jsonObject["match"]["venue"].ToString();
            match.HomeTeamName = jsonObject["match"]["homeTeam"]["name"].ToString();
            match.HomeTeamId = int.Parse(jsonObject["match"]["homeTeam"]["id"].ToString());
            match.AwayTeamName = jsonObject["match"]["awayTeam"]["name"].ToString();
            match.AwayTeamId = int.Parse(jsonObject["match"]["awayTeam"]["id"].ToString());
            match.MatchId = jsonObject["match"]["id"].ToString();
            match.UtcDate = jsonObject["match"]["utcDate"].ToString();
            match.LeagueId = int.Parse(jsonObject["match"]["competition"]["id"].ToString());
            match.LeagueName = jsonObject["match"]["competition"]["name"].ToString();

            return match;
        }

        private Match populateFinishedMatch(JObject jsonObject, Match match)
        {
            match.finished = true;
            match.Stadium = jsonObject["match"]["venue"].ToString();
            match.HomeTeamName = jsonObject["match"]["homeTeam"]["name"].ToString();
            match.HomeTeamId = int.Parse(jsonObject["match"]["homeTeam"]["id"].ToString());
            match.AwayTeamName = jsonObject["match"]["awayTeam"]["name"].ToString();
            match.AwayTeamId = int.Parse(jsonObject["match"]["awayTeam"]["id"].ToString());
            match.UtcDate = jsonObject["match"]["utcDate"].ToString();
            match.LeagueId = int.Parse(jsonObject["match"]["competition"]["id"].ToString());
            match.LeagueName = jsonObject["match"]["competition"]["name"].ToString();
            match.homeGoals = int.Parse(jsonObject["match"]["score"]["fullTime"]["homeTeam"].ToString());
            match.awayGoals = int.Parse(jsonObject["match"]["score"]["fullTime"]["awayTeam"].ToString());
            JArray goals = (JArray)jsonObject["match"]["goals"];
            match.homeGoalScorers = populateGoalScorers(goals, match.HomeTeamId);
            match.awayGoalScorers = populateGoalScorers(goals, match.AwayTeamId);
            match.resultRetrieved = true;

            return match;
        }

        private Match getMatchFromDatabase(SqlConnectionStringBuilder builder, Match match)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {

                using (SqlCommand sqlCommand = new SqlCommand("SELECT * from Match as match " +
                        "Where match.MatchId = " + match.MatchId + "; ", connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        reader.Read();
                        match.HomeTeamId = (int)reader["HomeTeamId"];
                        match.HomeTeamName = reader["HomeTeamName"].ToString();
                        match.AwayTeamId = (int)reader["AwayTeamId"];
                        match.AwayTeamName = reader["AwayTeamName"].ToString();
                        match.UtcDate = reader["UTCDate"].ToString();
                        match.LeagueName = reader["LeagueName"].ToString();
                        match.Stadium = reader["Stadium"].ToString();
                        match.LeagueId = (int)reader["LeagueId"];
                        match.predictedResult = (int)reader["PredictedResult"];
                        match.predictionString = reader["PredictionString"].ToString();
                        match.predictedScore = new int[] { (int)reader["PredictedHomeGoals"], (int)reader["PredictedAwayGoals"] };
                        match.finished = (bool)reader["Finished"];
                        match.predictionMade = (bool)reader["PredictionMade"];
                        match.resultRetrieved = (bool)reader["ResultRetrieved"];
                        match.homeGoals = (int)reader["HomeGoalsResult"];
                        match.awayGoals = (int)reader["AwayGoalsResult"];
                        match.predictionResultRecorded = (bool)reader["PredictionResultRecorded"];
                        reader.Close();
                    }
                    connection.Close();

                }
            }
            if (!match.finished)
            {
                match.HomeStats = getStatsFromDatabase(builder, match.HomeTeamId, match);
                match.AwayStats = getStatsFromDatabase(builder, match.AwayTeamId, match);
            }
            else
            {
                match.homeGoalScorers = getGoalsFromDatabase(builder, match.HomeTeamId, match);
                match.awayGoalScorers = getGoalsFromDatabase(builder, match.AwayTeamId, match);
            }

            return match;
        }

        private MatchStats getStatsFromDatabase(SqlConnectionStringBuilder builder, int teamId, Match match)
        {
            MatchStats stats = new MatchStats();
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {

                using (SqlCommand sqlCommand = new SqlCommand("SELECT * from MatchStats as stats " +
                        "Where stats.MatchId = " + match.MatchId + " AND stats.TeamId = " + teamId + "; ", connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        reader.Read();
                        stats.position = reader["Position"].ToString();
                        stats.HomeOrAwayPosition = reader["HomeOrAwayPosition"].ToString();
                        stats.name = reader["Name"].ToString();
                        stats.matchesPlayed = (int)reader["MatchesPlayed"];
                        stats.won = (int)reader["Won"];
                        stats.drawn = (int)reader["Drawn"];
                        stats.lost = (int)reader["Lost"];
                        stats.homeOrAwayMatchesPlayed = (int)reader["HomeOrAwayMatchesPlayed"];
                        stats.homeOrAwayWon = (int)reader["HomeOrAwayWon"];
                        stats.homeOrAwayDrawn = (int)reader["HomeOrAwayDrawn"];
                        stats.homeOrAwayLost = (int)reader["HomeOrAwayLost"];
                        stats.goalsFor = (int)reader["GoalsFor"];
                        stats.goalsAgainst = (int)reader["GoalsAgainst"];
                        stats.goalDifference = (int)reader["GoalsDifference"];
                        stats.points = (int)reader["Points"];
                        stats.goalsScoredPerGame = (double)reader["GoalsScoredPerGame"];
                        stats.goalsConcededPerGame = (double)reader["GoalsConcededPerGame"];
                        stats.WDL = reader["WDL"].ToString();
                        stats.homeOrAwayWDL = reader["HomeOrAwayWDL"].ToString();
                        stats.overallFormString = reader["OverallFormString"].ToString();
                        stats.homeOrAwayFormString = reader["HomeOrAwayFormString"].ToString();
                        stats.PredictionPoints = (double)reader["PredictionPoints"];
                        stats.overallLast6Scored = (int)reader["OverallLast6Scored"];
                        stats.overallLast6Conceded = (int)reader["OverallLast6Conceded"];
                        stats.homeOrAwayLast6Scored = (int)reader["HomeOrAwayLast6Scored"];
                        stats.homeOrAwayLast6Conceded = (int)reader["HomeOrAwayLast6Conceded"];
                        stats.teamId = (int)reader["TeamId"];
                        stats.matchId = reader["MatchId"].ToString();
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return stats;
        }

        private List<Goal> getGoalsFromDatabase(SqlConnectionStringBuilder builder, int teamId, Match match)
        {
            List<Goal> goals = new List<Goal>();
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("SELECT * from Goal as goal " +
                        "Where goal.MatchId = " + match.MatchId + " AND goal.TeamId = " + teamId + "; ", connection))
                {
                    connection.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Goal goal = new Goal();
                        goal.minute = (int)reader["Minute"];
                        goal.scorer = reader["Scorer"].ToString();
                        goal.assist = reader["Assist"].ToString();
                        goals.Add(goal);
                    }
                    connection.Close();
                }
            }
            return goals;
        }

        private List<Goal> populateGoalScorers(JArray goals, int teamId)
        {
            List<Goal> goalScorers = new List<Goal>();
            for (int i = 0; i < goals.Count; i++)
            {
                if ((int.Parse(goals[i]["team"]["id"].ToString()) == teamId && goals[i]["type"].ToString() != "OWN")
                    || int.Parse(goals[i]["team"]["id"].ToString()) != teamId && goals[i]["type"].ToString() == "OWN")
                {
                    Goal goal = new Goal
                    {
                        minute = int.Parse(goals[i]["minute"].ToString()),
                        scorer = goals[i]["scorer"]["name"].ToString()
                    };
                    if (goals[i]["type"].ToString() == "OWN")
                    {
                        goal.scorer += " (OG)";
                    }
                    if (goals[i]["type"].ToString() == "PENALTY")
                    {
                        goal.scorer += " (Penalty)";
                    }
                    if (goals[i]["assist"].ToString() != "")
                    {
                        goal.assist = "(" + goals[i]["assist"]["name"].ToString() + ")";
                    }
                    goalScorers.Add(goal);
                }
            }
            return goalScorers;
        }
    }
}
