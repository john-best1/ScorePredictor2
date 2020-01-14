using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScorePredictor.Data
{
    public static class Predictor
    {
        public static Match generatePrediction(Match match)
        {
            match.HomeStats.PredictionPoints = 2.5 + calculatePoints(match.HomeStats.overallLastSix);
            match.AwayStats.PredictionPoints = calculatePoints(match.AwayStats.overallLastSix);

            match.predictedResult = predictResult(match.HomeStats.PredictionPoints, match.AwayStats.PredictionPoints);

            predictionString(match);
            match.predictedScore = predictScore(match);

            return match;
        }

        private static int[] predictScore(Match match)
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
                if (homeGoals < decimalHomeGoals)
                {
                    homeGoals++;
                } //homeGoals 3, awaygoals2;
                if (match.predictedResult == 2 && homeGoals <= awayGoals)
                {
                    if (awayGoals > decimalAwayGoals && homeGoals > 0)
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
            else if (match.predictedResult == 1)
            {
                if (awayGoals > decimalAwayGoals)
                {
                    awayGoals--;
                }
                if (homeGoals < decimalHomeGoals)
                {
                    homeGoals++;
                }
                if (homeGoals == awayGoals)
                {
                    homeGoals++;
                    awayGoals--;
                }
                else if (homeGoals - awayGoals == 1)
                {
                    homeGoals++;
                }
                else
                {
                    if (homeGoals <= awayGoals)
                    {
                        if (homeGoals > 0)
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
            else if (match.predictedResult == 3 && homeGoals != awayGoals)
            {
                System.Diagnostics.Debug.WriteLine("---------\n" + homeGoals + " " + decimalHomeGoals + "\n----------");
                if (homeGoals > decimalHomeGoals && homeGoals > 0)
                {
                    homeGoals--;
                }
                if (homeGoals != awayGoals && awayGoals < decimalAwayGoals)
                {
                    awayGoals++;
                }
                if (homeGoals != awayGoals)
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

        private static void predictionString(Match match)
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

        private static int predictResult(double homePoints, double awayPoints)
        {
            // 1 = strong home win, 2 = regular home win, 3 = draw, 4 = strong away win, 5 = regular away win
            if (homePoints - awayPoints > 9.0)
            {
                return 1;
            }
            else if (homePoints - awayPoints > 2.5)
            {
                return 2;
            }
            else if (awayPoints - homePoints > 9.0)
            {
                return 4;
            }
            else if (awayPoints - homePoints > 2.5)
            {
                return 5;
            }
            else
            {
                return 3;
            }
        }

        private static double calculatePoints(int[] last6)
        {
            double total = 0;
            foreach (int result in last6)
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
    }
}
