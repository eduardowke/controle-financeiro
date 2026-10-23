namespace ControleFinanceiro.Domain;

public static class VectorMath
{
    /// <summary>
    /// Calcula a similaridade de cosseno entre dois vetores de mesma dimensão.
    /// Utiliza ReadOnlySpan<float> para performance de alto nível e zero alocação de memória.
    /// </summary>

    public static float CosineSimilarity(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Os vetores devem possuir exatamente a mesma dimensão para o cálculo.");

        if (vectorA.Length == 0)
            return 0.0f;

        float dotProduct = 0.0f;
        float normA = 0.0f;
        float normB = 0.0f;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        if (normA == 0.0f || normB == 0.0f)
            return 0.0f;

        return dotProduct / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }
}