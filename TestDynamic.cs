using System;
using System.Collections.Generic;

public readonly struct ErrorOr<T> {
    public List<string> Errors { get; }
    private ErrorOr(List<string> errors) { Errors = errors; }
    public static implicit operator ErrorOr<T>(List<string> errors) => new ErrorOr<T>(errors);
}

class Program {
    static void Main() {
        try {
            var result = Run<ErrorOr<Guid>>();
            Console.WriteLine("Success: " + (result is ErrorOr<Guid>));
        } catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
    
    static TResponse Run<TResponse>() {
        var errors = new List<string> { "error" };
        return (dynamic)errors;
    }
}
