﻿// See https://aka.ms/new-console-template for more information

using Lib;
using QuickSpike;
using System.Text;

const int IndentSize = 2;

string remoteAddress = "http://localhost:8585";
if (args.Length > 0)
{
    remoteAddress = args[0];
}

RemoteConnector remoteConnector = new(remoteAddress);
Evaluator eval = new Evaluator(remoteConnector);

Console.WriteLine($"Remote address {remoteAddress}");

int pendingBraces = 0;
StringBuilder sourceBuffer = new();

while (true)
{
    string input = GetNextInput(indentLevel: pendingBraces).Trim();
    if (input == ".exit")
    {
        Console.WriteLine("Bye!");
        break;
    }

    if (input == "")
    {
        continue;
    }

    if (input.EndsWith('{'))
    {
        pendingBraces++;
    }

    if (input.EndsWith('}'))
    {
        pendingBraces--;
    }

    if (sourceBuffer.Length > 0)
    {
        sourceBuffer.AppendLine();
    }

    sourceBuffer.Append(input);

    if (pendingBraces > 0)
    {
        continue;
    }

    try
    {
        string source = sourceBuffer.ToString();
        sourceBuffer = new();
        object value = eval.Execute(source);
        Console.WriteLine(value);
    }
    catch (Exception e)
    {
        Console.WriteLine($"ERROR: {e.Message}");
    }
    
}

string GetNextInput(string prompt = ">> ", int indentLevel = 0)
{
    Console.Write(prompt);
    Indent(indentLevel, IndentSize);
    string? input = Console.ReadLine();
    if (input == null)
    {
        throw new Exception("Read null");
    }

    return input;
}

void Indent(int indentLevel, int indentSize)
{
    char[] buffer = new char[indentLevel * indentSize];
    Array.Fill(buffer, ' ');
    Console.Write(buffer);
}


