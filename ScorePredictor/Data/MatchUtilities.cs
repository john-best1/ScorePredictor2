using Newtonsoft.Json.Linq;
using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScorePredictor.Data
{
    public static class MatchUtilities
    {
        
        //returns a string like "33/50/17", representing win/draw/loss percentage
        static public string getWDLString(MatchStats stats)
        {
            string won = Math.Round((double)stats.won / stats.matchesPlayed * 100.0, 0).ToString();
            string drawn = Math.Round((double)stats.drawn / stats.matchesPlayed * 100.0, 0).ToString();
            string lost = Math.Round((double)stats.lost / stats.matchesPlayed * 100.0, 0).ToString();
            stats.WDL = won + "/" + drawn + "/" + lost;
            won = Math.Round((double)stats.homeOrAwayWon / stats.homeOrAwayMatchesPlayed * 100.0, 0).ToString();
            drawn = Math.Round((double)stats.homeOrAwayDrawn / stats.homeOrAwayMatchesPlayed * 100.0, 0).ToString();
            lost = Math.Round((double)stats.homeOrAwayLost / stats.homeOrAwayMatchesPlayed * 100.0, 0).ToString();
            return won + "/" + drawn + "/" + lost;
        }
        // returns a "WDLLDW" type string from an array like [1,3,4,2,3,1](representing last 6 results/home or away results)
        public static string getWDLStringFromArray(int[] results)
        {
            string wdl = "";
            foreach (int result in results)
            {
                if (result == 1 || result == 2)
                {
                    wdl += "W";
                }
                else if (result == 3 || result == 4)
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

        // generates stats team stats
        public static MatchStats getTeamForm(MatchStats stats, int teamId, JArray allMatches, bool home = true)
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
            for (int i = 1; i <= allMatches.Count; i++)
            {
                if (int.Parse(allMatches[allMatches.Count - i]["homeTeam"]["id"].ToString()) == teamId)
                {
                    switch (allMatches[allMatches.Count - i]["score"]["winner"].ToString())
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
                            if (added < 6)
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
                            if (added < 6)
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
                            if (added < 6)
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
                            if (added < 6)
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
                            if (added < 6)
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

            return stats;
        }
    }
}
