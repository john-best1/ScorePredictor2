using Newtonsoft.Json.Linq;
using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.Data
{
    public class MatchService
    {
        Match match = new Match();
        LeagueService leagueService = new LeagueService();
        HttpClient client = new HttpClient();
        int season = 2019;   //change this every season
        bool matchInDatabase = false;
        string flagUrl;


        public async Task<Match> getMatch(string matchId, string flagUrl)
        {
            this.flagUrl = flagUrl;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "scorepredictordb.database.windows.net";
            builder.UserID = "jbest";
            builder.Password = "databasepassword*1";
            builder.InitialCatalog = "scorepredictordb";
            match.MatchId = matchId;
            matchInDatabase = databaseCheck(builder);
            if (matchInDatabase)
            {
                getMatchDataFromDatabase(builder);
                return match;
            }
            else
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");
                using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/matches/" + matchId))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());

                        populateMatch(jsonObject, builder);

                        match.HomeStats = await leagueService.getStats(match.LeagueId, match.HomeTeamId, match.MatchId);
                        match.AwayStats = await leagueService.getStats(match.LeagueId, match.AwayTeamId, match.MatchId, false);
                        getWDLString(match.HomeStats);
                        getWDLString(match.AwayStats);
                        match = await getRecentForm(match);
                        addMatchToDatabase(builder);
                        return match;
                    }
                    else
                    {
                        throw new Exception(response.ReasonPhrase);
                    }
                }
            }
        }

        private void addMatchToDatabase(SqlConnectionStringBuilder builder)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("insert into Match values(" +
                "@matchid,@hometeamid,@hometeamname,@awayteamid,@awayteamname,@utcdate,@leaguename,@stadium," +
                "@leagueid,@imageurl,@predictedresult,@predictionstring,@predictedhomescore,@predictedawayscore," +
                "@finished,@predictionmade,@resultretrieved, @homegoalsscored, @awaygoalsscored);", connection))
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


                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            addStatsToDatabase(builder, match.HomeStats, match.HomeTeamId);
            addStatsToDatabase(builder, match.AwayStats, match.AwayTeamId);
            addGoalScorersToDatabase(builder, match.homeGoalScorers, match.HomeTeamId);
            addGoalScorersToDatabase(builder, match.awayGoalScorers, match.AwayTeamId);
        }

        private void addStatsToDatabase(SqlConnectionStringBuilder builder, MatchStats stats, int teamId)
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
                    cmd.Parameters.AddWithValue("@name",stats.name);
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
                    cmd.Parameters.AddWithValue("@matchid", match.MatchId);


                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private void addGoalScorersToDatabase(SqlConnectionStringBuilder builder, List<Goal> goalScorers, int teamId)
        {
            if (goalScorers.Any())
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    foreach(Goal goal in goalScorers)
                    {
                        using (SqlCommand cmd = new SqlCommand("insert into Goal values(" +
                            "@minute, @scorer, @assist, @matchid, @teamid);", connection))
                        {
                            cmd.Parameters.AddWithValue("@minute", goal.minute);
                            cmd.Parameters.AddWithValue("@scorer", goal.scorer);
                            cmd.Parameters.AddWithValue("@assist", goal.assist);
                            cmd.Parameters.AddWithValue("@matchid", match.MatchId);
                            cmd.Parameters.AddWithValue("@teamid", teamId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
            }
        }

        private void getMatchDataFromDatabase(SqlConnectionStringBuilder builder)
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
                        match.ImageUrl = reader["ImageURL"].ToString();
                        match.predictedResult = (int)reader["PredictedResult"];
                        match.predictionString = reader["PredictionString"].ToString();
                        match.predictedScore =new int[] { (int)reader["PredictedHomeGoals"], (int)reader["PredictedAwayGoals"]};
                        match.finished = (bool)reader["Finished"];
                        match.predictionMade = (bool)reader["PredictionMade"];
                        match.resultRetrieved = (bool)reader["ResultRetrieved"];
                        match.homeGoals = (int)reader["HomeGoalsResult"];
                        match.awayGoals = (int)reader["AwayGoalsResult"];
                        reader.Close();
                    }
                    connection.Close();

                }
            }
            match.HomeStats = getStatsFromDatabase(builder, match.HomeTeamId);
            match.AwayStats = getStatsFromDatabase(builder, match.AwayTeamId);
            match.homeGoalScorers = getGoalsFromDatabase(builder, match.HomeTeamId);
            match.awayGoalScorers = getGoalsFromDatabase(builder, match.AwayTeamId);
        }

        private List<Goal> getGoalsFromDatabase(SqlConnectionStringBuilder builder, int teamId)
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

        private MatchStats getStatsFromDatabase(SqlConnectionStringBuilder builder, int teamId)
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

        private bool databaseCheck(SqlConnectionStringBuilder builder)
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

        public async Task<Match> getRecentForm(Match match)
        {
            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/competitions/" + match.LeagueId + "/matches?status=FINISHED&season=" + season ))
            {
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    JArray allCompetitionMatches = (JArray)jsonObject["matches"];
                    getTeamForm(match.HomeStats, match.HomeTeamId, allCompetitionMatches);
                    getTeamForm(match.AwayStats, match.AwayTeamId, allCompetitionMatches, false);
                    formStrings(match.HomeStats, match.AwayStats);
                    generatePrediction(match);
                    return match;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        private void generatePrediction(Match match)
        {
            match.HomeStats.PredictionPoints = 2.5 + calculatePoints(match.HomeStats.overallLastSix);
            match.AwayStats.PredictionPoints = calculatePoints(match.AwayStats.overallLastSix);

            match.predictedResult = predictResult(match.HomeStats.PredictionPoints, match.AwayStats.PredictionPoints);

            predictionString(match);
            match.predictedScore = predictScore(match);
        }

        private int[] predictScore(Match match)
        {
            int[] predictedResult = new int[2];

            int homeGoals = (int)Math.Round(match.HomeStats.overallLast6Scored / 6.0, 0);
            int awayGoals = (int)Math.Round(match.AwayStats.overallLast6Scored / 6.0, 0);
            double decimalHomeGoals = match.HomeStats.overallLast6Scored / 6.0;
            double decimalAwayGoals = match.AwayStats.overallLast6Scored / 6.0;

            //int homeGoals = (int)Math.Round(match.HomeStats.homeOrAwayLast6Scored / 6.0, 0);
            //int awayGoals = (int)Math.Round(match.AwayStats.homeOrAwayLast6Scored / 6.0, 0);
            //double decimalHomeGoals = match.HomeStats.homeOrAwayLast6Scored / 6.0;
            //double decimalAwayGoals = match.AwayStats.homeOrAwayLast6Scored / 6.0;


            //regular home win
            if (match.predictedResult == 2 && homeGoals <= awayGoals)
            {
                if (awayGoals > decimalAwayGoals)
                {
                    awayGoals--;
                } //homeGoals 2, awaygoals2;
                if(homeGoals < decimalHomeGoals)
                {
                    homeGoals++;
                } //homeGoals 3, awaygoals2;
                if (match.predictedResult == 2 && homeGoals <= awayGoals)
                {
                    if(awayGoals > decimalAwayGoals && homeGoals > 0)
                    {
                        awayGoals = homeGoals - 1;
                    }
                    else
                    {
                        homeGoals = awayGoals + 1;
                    }
                }
            }

            //strong home win
            else if(match.predictedResult == 1)
            {
                if(awayGoals > decimalAwayGoals)
                {
                    awayGoals--;
                }
                if(homeGoals < decimalHomeGoals)
                {
                    homeGoals++;
                }
                if(homeGoals == awayGoals)
                {
                    homeGoals++;
                    awayGoals--;
                }
                else if(homeGoals - awayGoals == 1)
                {
                    homeGoals++;
                }
                else
                {
                    if(homeGoals <= awayGoals)
                    {
                        if(homeGoals > 0)
                        {
                            awayGoals = homeGoals - 1;
                            homeGoals++;
                        }
                        else
                        {
                            homeGoals = awayGoals + 2;
                        }
                    }
                }
            }

            //strong away win
            else if (match.predictedResult == 4)
            {
                if (homeGoals > decimalHomeGoals)
                {
                    homeGoals--;
                }
                if (awayGoals < decimalAwayGoals)
                {
                    awayGoals++;
                }
                if (homeGoals == awayGoals)
                {
                    homeGoals--;
                    awayGoals++;
                }
                else if (awayGoals - homeGoals == 1)
                {
                    awayGoals++;
                }
                else
                {
                    if (awayGoals <= homeGoals)
                    {
                        if (awayGoals > 0)
                        {
                            homeGoals = awayGoals - 1;
                            awayGoals++;
                        }
                        else
                        {
                            awayGoals = homeGoals + 2;
                        }
                    }
                }
            }

            //regular away win
            if (match.predictedResult == 5 && awayGoals <= homeGoals)
            {
                if (homeGoals > decimalHomeGoals)
                {
                    homeGoals--;
                }
                if (awayGoals < decimalAwayGoals)
                {
                    homeGoals++;
                }
                if (match.predictedResult == 5 && awayGoals <= homeGoals)
                {
                    if (homeGoals > decimalHomeGoals && awayGoals > 0)
                    {
                        homeGoals = awayGoals - 1;
                    }
                    else
                    {
                        awayGoals = homeGoals + 1;
                    }
                }
            }

            //draw
            else if(match.predictedResult == 3 && homeGoals != awayGoals)
            {
                System.Diagnostics.Debug.WriteLine("---------\n" + homeGoals + " " + decimalHomeGoals + "\n----------");
                if(homeGoals > decimalHomeGoals && homeGoals > 0)
                {
                    homeGoals--;
                }
                if(homeGoals != awayGoals && awayGoals < decimalAwayGoals)
                {
                    awayGoals++;
                }
                if(homeGoals != awayGoals)
                {
                    if (homeGoals > awayGoals)
                    {
                        awayGoals = homeGoals;
                    }
                    else
                    {
                        homeGoals = awayGoals;
                    }
                }
            }

            predictedResult[0] = homeGoals;
            predictedResult[1] = awayGoals;
            match.predictionMade = true;
            return predictedResult;
        }

        private void predictionString(Match match)
        {
            switch (match.predictedResult)
            {
                case 1:
                    match.predictionString = "STRONG HOME WIN";
                    break;
                case 2:
                    match.predictionString = "HOME WIN";
                    break;
                case 3:
                    match.predictionString = "DRAW";
                    break;
                case 4:
                    match.predictionString = "STRONG AWAY WIN";
                    break;
                case 5:
                    match.predictionString = "AWAY WIN";
                    break;
                default:
                    break;
            }
        }

        private int predictResult(double homePoints, double awayPoints)
        {
           // 1 = strong home win, 2 = regular home win, 3 = draw, 4 = strong away win, 5 = regular away win
            if(homePoints - awayPoints > 9.0)
            {
                return 1;
            }
            else if (homePoints - awayPoints > 2.5)
            {
                return 2;
            }
            else if(awayPoints - homePoints > 9.0)
            {
                return 4;
            }
            else if(awayPoints - homePoints > 2.5)
            {
                return 5;
            }
            else
            {
                return 3;
            }
        }

        private double calculatePoints(int[] last6)
        {
            double total = 0;
            foreach(int result in last6)
            {
                switch (result)
                {
                    case 1:
                        total += 3;
                        break;
                    case 2:
                        total += 4.5;
                        break;
                    case 3:
                        total += 1;
                        break;
                    case 4:
                        total += 1.5;
                        break;
                    default:
                        break;
                }
            }
            return total;
        }

        private void getWDLString(MatchStats stats)
        {
            string won = Math.Round((double)stats.won / stats.matchesPlayed * 100.0, 0).ToString();
            string drawn = Math.Round((double)stats.drawn / stats.matchesPlayed * 100.0, 0).ToString();
            string lost = Math.Round((double)stats.lost / stats.matchesPlayed * 100.0, 0).ToString();
            stats.WDL = won + "/" + drawn + "/" + lost;
            won = Math.Round((double)stats.homeOrAwayWon / stats.homeOrAwayMatchesPlayed * 100.0, 0).ToString();
            drawn = Math.Round((double)stats.homeOrAwayDrawn / stats.homeOrAwayMatchesPlayed * 100.0, 0).ToString();
            lost = Math.Round((double)stats.homeOrAwayLost / stats.homeOrAwayMatchesPlayed * 100.0, 0).ToString();
            stats.homeOrAwayWDL = won + "/" + drawn + "/" + lost;
        }

        private void formStrings(MatchStats home, MatchStats away)
        {
            home.overallFormString = getWDLStringFromArray(home.overallLastSix);
            away.overallFormString = getWDLStringFromArray(away.overallLastSix);
        }

        private string getWDLStringFromArray(int[] results)
        {
            string wdl = "";
            foreach(int result in results)
            {
                if (result == 1 || result == 2)
                {
                    wdl += "W";
                }else if(result == 3 || result == 4)
                {
                    wdl += "D";
                }
                else
                {
                    wdl += "L";
                }
            }
            return wdl;
        }

        private void populateMatch(JObject jsonObject, SqlConnectionStringBuilder builder)
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
            match.ImageUrl = flagUrl;

            if (jsonObject["match"]["status"].ToString() == "FINISHED")
            {
                populateResult(jsonObject);
            }
        }

        private void populateResult(JObject jsonObject)
        {
            match.finished = true;
            match.homeGoals = int.Parse(jsonObject["match"]["score"]["fullTime"]["homeTeam"].ToString());
            match.awayGoals = int.Parse(jsonObject["match"]["score"]["fullTime"]["awayTeam"].ToString());
            JArray goals = (JArray)jsonObject["match"]["goals"];
            match.homeGoalScorers = populateGoalScorers(goals, match.HomeTeamId);
            match.awayGoalScorers = populateGoalScorers(goals, match.AwayTeamId);
            match.resultRetrieved = true;
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

        public void getTeamForm(MatchStats stats, int teamId, JArray allMatches, bool home = true)
        {
            //1 = home win, 2 = away win, 3 = home draw, 4 = away draw, 5 = loss
            int[] results = new int[6];
            int[] homeOrAwayResults = new int[6];
            int added = 0;
            int homeOrAwayAdded = 0;
            int overallGoalsScored = 0;
            int overallGoalsConceded = 0;
            int homeOrAwayGoalsScored = 0;
            int homeOrAwayGoalsConceded = 0;
            for(int i = 1; i <= allMatches.Count; i++)
            {
                if(int.Parse(allMatches[allMatches.Count - i]["homeTeam"]["id"].ToString()) == teamId)
                {
                    switch(allMatches[allMatches.Count - i]["score"]["winner"].ToString())
                    {
                        case "HOME_TEAM":
                            if (added < 6)
                            {
                                results[5 - added] = 1;
                                added++;
                                overallGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                                overallGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                            }
                            if (home && homeOrAwayAdded < 6)
                            {
                                homeOrAwayResults[5 - homeOrAwayAdded] = 1;
                                homeOrAwayAdded++;
                                homeOrAwayGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                                homeOrAwayGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                            }
                            break;
                        case "DRAW":
                            if(added < 6)
                            {
                                results[5 - added] = 3;
                                added++;
                                overallGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                                overallGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                            }
                            if (home && homeOrAwayAdded < 6)
                            {
                                homeOrAwayResults[5 - homeOrAwayAdded] = 3;
                                homeOrAwayAdded++;
                                homeOrAwayGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                                homeOrAwayGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                            }
                            break;
                        default:
                            if(added < 6)
                            {
                                results[5 - added] = 5;
                                added++;
                                overallGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                                overallGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                            }
                            if (home && homeOrAwayAdded < 6)
                            {
                                homeOrAwayResults[5 - homeOrAwayAdded] = 5;
                                homeOrAwayAdded++;
                                homeOrAwayGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                                homeOrAwayGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                            }
                            break;
                    }
                }
                else if (int.Parse(allMatches[allMatches.Count - i]["awayTeam"]["id"].ToString()) == teamId)
                {
                    switch (allMatches[allMatches.Count - i]["score"]["winner"].ToString())
                    {
                        case "AWAY_TEAM":
                            if(added < 6)
                            {
                                results[5 - added] = 2;
                                added++;
                                homeOrAwayGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                                homeOrAwayGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                            }
                            if (!home && homeOrAwayAdded < 6)
                            {
                                homeOrAwayResults[5 - homeOrAwayAdded] = 2;
                                homeOrAwayAdded++;
                                homeOrAwayGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                                homeOrAwayGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                            }
                            break;
                        case "DRAW":
                            if(added < 6)
                            {
                                results[5 - added] = 4;
                                added++;
                                homeOrAwayGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                                homeOrAwayGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                            }
                            if (!home && homeOrAwayAdded < 6)
                            {
                                homeOrAwayResults[5 - homeOrAwayAdded] = 4;
                                homeOrAwayAdded++;
                                homeOrAwayGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                                homeOrAwayGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                            }
                            break;
                        default:
                            if(added < 6)
                            {
                                results[5 - added] = 5;
                                added++;
                                homeOrAwayGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                                homeOrAwayGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                            }
                            if (!home && homeOrAwayAdded < 6)
                            {
                                homeOrAwayResults[5 - homeOrAwayAdded] = 5;
                                homeOrAwayAdded++;
                                homeOrAwayGoalsScored += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["awayTeam"].ToString());
                                homeOrAwayGoalsConceded += int.Parse(allMatches[allMatches.Count - i]["score"]["fullTime"]["homeTeam"].ToString());
                            }
                            break;
                    }
                }
                if (added == 6 && homeOrAwayAdded == 6) break;
            }
            stats.overallLastSix = results;
            stats.overallFormString = getWDLStringFromArray(results);
            stats.homeOrAwayLastSix = homeOrAwayResults;
            stats.homeOrAwayFormString = getWDLStringFromArray(homeOrAwayResults);
            stats.homeOrAwayLast6Scored = homeOrAwayGoalsScored;
            stats.homeOrAwayLast6Conceded = homeOrAwayGoalsConceded;
            stats.overallLast6Conceded = overallGoalsConceded;
            stats.overallLast6Scored = overallGoalsScored;
        }
    }
}
