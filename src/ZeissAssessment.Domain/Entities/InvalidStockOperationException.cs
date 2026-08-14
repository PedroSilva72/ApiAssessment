namespace ZeissAssessment.Domain.Entities;

public class InvalidStockOperationException(string message) : Exception(message);
