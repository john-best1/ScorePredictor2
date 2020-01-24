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

            match.predictionString = predictionString(match.predictedResult);
            match.predictedScore = predictScore(match.HomeStats.overallLast6Scored, match.AwayStats.overallLast6Scored, match.predictedResult);
            match.predictionMade = true;

            return match;
        }

        public static int[] predictScore(int homeLast6Scored, int awayLast6Scored, int prediction)
        {
            int[] predictedResult = new int[2];

            int homeGoals = (int)Math.Round(homeLast6Scored / 6.0, 0);
            int awayGoals = (int)Math.Round(awayLast6Scored / 6.0, 0);
            double decimalHomeGoals = homeLast6Scored / 6.0;
            double decimalAwayGoals = awayLast6Scored / 6.0;

            //int homeGoals = (int)Math.Round(match.HomeStats.homeOrAwayLast6Scored / 6.0, 0);
            //int awayGoals = (int)Math.Round(match.AwayStats.homeOrAwayLast6Scored / 6.0, 0);
            //double decimalHomeGoals = match.HomeStats.homeOrAwayLast6Scored / 6.0;
            //double decimalAwayGoals = match.AwayStats.homeOrAwayLast6Scored / 6.0;


            //regular home win
            if (prediction == 2 && homeGoals <= awayGoals)
            {
                if (awayGoals > decimalAwayGoals)
                {
                    awayGoals--;
                } 
                if (homeGoals < decimalHomeGoals)
                {
                    homeGoals++;
                } 
                if (prediction == 2 && homeGoals <= awayGoals)
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
            else if (prediction == 1)
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
                    if (awayGoals > 0) awayGoals--;
                    else if (awayGoals == 0) homeGoals++;
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
            else if (prediction == 4)
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
                    awayGoals++;
                    if (homeGoals > 0) homeGoals--;
                    else if (homeGoals == 0) awayGoals++;
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
            if (prediction == 5 && awayGoals <= homeGoals)
            {
                if (homeGoals > decimalHomeGoals)
                {
                    homeGoals--;
                }
                if (awayGoals < decimalAwayGoals)
                {
                    homeGoals++;
                }
                if (prediction == 5 && awayGoals <= homeGoals)
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
            else if (prediction == 3 && homeGoals != awayGoals)
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
            return predictedResult;
        }

        public static string predictionString(int predictedResult)
        {
            switch (predictedResult)
            {
                case 1:
                    return "STRONG HOME WIN";
                case 2:
                    return "HOME WIN";
                case 3:
                    return "DRAW";
                case 4:
                    return "STRONG AWAY WIN";
                case 5:
                    return "AWAY WIN";
                default:
                    break;
            }
            return "Error";
        }

        public static int predictResult(double homePoints, double awayPoints)
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

        public static double calculatePoints(int[] last6)
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
