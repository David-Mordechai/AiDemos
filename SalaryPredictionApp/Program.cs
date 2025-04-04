using Microsoft.ML;
using Microsoft.ML.Data;

var mlContext = new MLContext();
var model = mlContext.Model.Load("SalaryModel.zip", out _);
var engine = mlContext.Model.CreatePredictionEngine<SalaryData, SalaryPrediction>(model);

var input = new SalaryData
{
    Experience = 5,
    EducationLevel = "Master",
    JobTitle = "Data Scientist"
};

var prediction = engine.Predict(input);
Console.WriteLine($"Predicted Salary: {prediction.Salary}");

internal class SalaryData
{
    [LoadColumn(0)]
    public float Experience { get; set; }

    [LoadColumn(1)]
    public string EducationLevel { get; set; } = default!;

    [LoadColumn(2)]
    public string JobTitle { get; set; } = default!;

    [LoadColumn(3)]
    public float Salary { get; set; }
}

internal class SalaryPrediction
{
    [ColumnName("Score")]
    public float Salary { get; set; }
}