using ScorePredictor.Models;
using System;
using Xunit;
using ScorePredictor.Data;

namespace ScorePredictor.Tests
{
    public class PredictorTests
    {
        //Match match = new Match();

        [Fact]
        // 1 = strong home win, 2 = home win, 3 = draw, 4 = strong away win, 5 = away win
        // difference > 9 = strong win, >= 3 and < 9 = regular win, < 3 = draw
        public void ShouldPredictStrongHomeWin()
        {
            int expectedResult = 1;
            int result = Predictor.predictResult(10, 0);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(17.5, 8);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(13.5, 2);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(23, 12);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(15, 5.5);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ShouldPredictRegularHomeWin()
        {
            int expectedResult = 2;
            int result = Predictor.predictResult(7, 0);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(15.5, 8);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(13.5, 7);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(23, 19);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(15, 6.5);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ShouldPredictDraw()
        {
            int expectedResult = 3;
            int result = Predictor.predictResult(7, 5);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(15.5, 14.5);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(13.5, 13.5);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(23, 24.5);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(15, 17.5);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ShouldPredictStrongAwayWin()
        {
            int expectedResult = 4;
            int result = Predictor.predictResult(7, 18);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(15.5, 25);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(13.5, 23);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(5, 19);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(5.5, 15);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ShouldPredictRegularAwayWin()
        {
            int expectedResult = 5;
            int result = Predictor.predictResult(7, 11);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(3, 8);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(13.5, 17);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(10, 19);

            Assert.Equal(expectedResult, result);

            result = Predictor.predictResult(0, 6.5);

            Assert.Equal(expectedResult, result);
        }


        [Fact]
        public void RegularHomeWinPredictedGoalsShouldBeHigher()
        {
            int[] predictedResult = Predictor.predictScore(10, 10, 2);

            Assert.True(predictedResult[0] > predictedResult[1] && predictedResult[0] > 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(0, 0, 2);

            Assert.True(predictedResult[0] > predictedResult[1] && predictedResult[0] > 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(20, 5, 2);

            Assert.True(predictedResult[0] > predictedResult[1] && predictedResult[0] > 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(4, 15, 2);

            Assert.True(predictedResult[0] > predictedResult[1] && predictedResult[0] > 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(0, 20, 2);

            Assert.True(predictedResult[0] > predictedResult[1] && predictedResult[0] > 0 && predictedResult[1] >= 0);
        }

        [Fact]
        public void StrongHomeWinPredictedGoalsShouldBeHigherByAtLeast2()
        {
            int[] predictedResult = Predictor.predictScore(10, 10, 1);

            Assert.True((predictedResult[0] - predictedResult[1] >= 2) && predictedResult[0] > 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(0, 0, 1);

            Assert.True((predictedResult[0] - predictedResult[1] >= 2) && predictedResult[0] > 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(20, 5, 1);

            Assert.True((predictedResult[0] - predictedResult[1] >= 2) && predictedResult[0] > 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(4, 15, 1);

            Assert.True((predictedResult[0] - predictedResult[1] >= 2) && predictedResult[0] > 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(0, 20, 1);

            Assert.True((predictedResult[0] - predictedResult[1] >= 2) && predictedResult[0] > 0 && predictedResult[1] >= 0);
        }

        [Fact]
        public void DrawPredictedGoalsShouldBeEqual()
        {
            int[] predictedResult = Predictor.predictScore(10, 10, 3);

            Assert.True(predictedResult[0] == predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(0, 0, 3);

            Assert.True(predictedResult[0] == predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(20, 5, 3);

            Assert.True(predictedResult[0] == predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(4, 15, 3);

            Assert.True(predictedResult[0] == predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] >= 0);

            predictedResult = Predictor.predictScore(0, 20, 3);

            Assert.True(predictedResult[0] == predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] >= 0);
        }

        [Fact]
        public void RegularAwayWinPredictedGoalsShouldBeHigher()
        {
            int[] predictedResult = Predictor.predictScore(10, 10, 5);

            Assert.True(predictedResult[0] < predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] > 0);

            predictedResult = Predictor.predictScore(0, 0, 5);

            Assert.True(predictedResult[0] < predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] > 0);

            predictedResult = Predictor.predictScore(20, 5, 5);

            Assert.True(predictedResult[0] < predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] > 0);

            predictedResult = Predictor.predictScore(4, 15, 5);

            Assert.True(predictedResult[0] < predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] > 0);

            predictedResult = Predictor.predictScore(0, 20, 5);

            Assert.True(predictedResult[0] < predictedResult[1] && predictedResult[0] >= 0 && predictedResult[1] > 0);
        }

        [Fact]
        public void StrongAwayWinPredictedGoalsShouldBeHigherByAtLeast2()
        {
            int[] predictedResult = Predictor.predictScore(10, 10, 4);

            Assert.True((predictedResult[1] - predictedResult[0] >= 2) && predictedResult[0] >= 0 && predictedResult[1] > 0);

            predictedResult = Predictor.predictScore(0, 0, 4);

            Assert.True((predictedResult[1] - predictedResult[0] >= 2) && predictedResult[0] >= 0 && predictedResult[1] > 0);

            predictedResult = Predictor.predictScore(20, 5, 4);

            Assert.True((predictedResult[1] - predictedResult[0] >= 2) && predictedResult[0] >= 0 && predictedResult[1] > 0);

            predictedResult = Predictor.predictScore(4, 15, 4);

            Assert.True((predictedResult[1] - predictedResult[0] >= 2) && predictedResult[0] >= 0 && predictedResult[1] > 0);

            predictedResult = Predictor.predictScore(0, 20, 4);

            Assert.True((predictedResult[1] - predictedResult[0] >= 2) && predictedResult[0] >= 0 && predictedResult[1] > 0);
        }
    }
}
