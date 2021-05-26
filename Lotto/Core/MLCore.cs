using System;
using System.Collections.Generic;
using Lotto.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Lotto.Core
{
    public class MLCore
    {
        public class Prediction
        {
            [ColumnName("PredictedNumber")]
            public float PredictedNumber { get; set; }
        }

        public int  GetPrediction( List<Two_Numbers> list, int predictNum)
        {
            MLContext mlContext = new MLContext();

            // 1. Import or create training data
            Two_Numbers[] houseData = list.ToArray();

            
            
            IDataView trainingData = mlContext.Data.LoadFromEnumerable(houseData);

            // 2. Specify data preparation and model training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "Num1" })
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Num2", maximumNumberOfIterations: 10000));

            // 3. Train model
            var model = pipeline.Fit(trainingData);

            // 4. Make a prediction
            var number = new Two_Numbers() { Num1 = predictNum };
            var price = mlContext.Model.CreatePredictionEngine<Two_Numbers, Prediction>(model).Predict(number);
            
            return (int)Math.Round(price.PredictedNumber);
        }
    }
}