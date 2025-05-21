using System;
using System.Reflection;

partial class Program
{
    static void Main()
    {
        string assemblyPath = "c:\\WorldofTrials\\Core\\bin\\Debug\\net9.0\\Core.dll";

        try
        {
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            Console.WriteLine($"Loaded Assembly: {assembly.FullName}\n");

            foreach (Type type in assembly.GetTypes())
            {
                Console.WriteLine($"Namespace: {type.Namespace}, Type: {type.Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}