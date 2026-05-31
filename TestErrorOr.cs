using System;
using ErrorOr;
using MediatR;
using Application.Commands.Publications;

class Program {
    static void Main() {
        bool implements = typeof(IErrorOr).IsAssignableFrom(typeof(ErrorOr<Guid>));
        Console.WriteLine("Implements IErrorOr: " + implements);
    }
}
