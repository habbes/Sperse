﻿namespace QuickSpike;

class Evaluator
{
    EvaluationContext context = new();

    public object Execute(string input)
    {
        Lexer lexer = new Lexer(input);
        using TokenStream tokens = lexer.Tokenize();
        Parser parser = new Parser(tokens);
        Expression expression = parser.ParseExpression();
        return expression.Evaluate(context);
    }
}


class EvaluationContext
{
    public SymbolTable SymbolTable { get; private set; } = new();
    public DelayedOperationTracker DelayedTracker { get; private set; }

    public EvaluationContext()
    {
        this.DelayedTracker = new(this);
    }
}

class SymbolTable
{
    Dictionary<string, object> symbols = new();

    public void SetSymbol(string id, object value)
    {
        symbols[id] = value;
    }

    public object GetSymbol(string id)
    {
        return symbols[id];
    }
}

class DelayedOperationTracker
{
    Dictionary<Guid, Entry> entries = new();
    EvaluationContext context;

    public DelayedOperationTracker(EvaluationContext context)
    {
        this.context = context;
    }
    
    public async Task DelayExecute(Guid id, ReactiveExpression wrapper, Expression expression)
    {
        Entry entry = new(id, expression);
        this.entries.Add(id, entry);
        await Task.Delay(4000);
        object value = expression.Evaluate(this.context);
        entry.Value = value;

        Console.WriteLine($"Expression Id {id} completed. Value = {value}");
        wrapper.Update(this.context);
        // propagate values
    }

    public void AddDependent(Guid parentId, Guid childId, Expression childExpression)
    {
        Entry entry = new(childId, childExpression);
        this.entries.Add(childId, entry);
        Entry parent = this.entries[parentId];
        parent.Dependents.Add(childId);
    }

    public object GetValue(Guid id)
    {
        Entry entry = this.entries[id];
        return entry.Value;
    }

    public void Work()
    {
        while (entries.Count > 0)
        {

        }
    }

    class Entry
    {
        public Entry(Guid id, Expression expression)
        {
            Id = id;
            Expression = expression;
            Value = new PendingValue(id);
        }
        public Guid Id { get; set; }
        public Expression Expression { get; set; }
        public List<Guid> Dependents { get; } = new();
        public Status Status { get; set; } = Status.Pending;
        public object Value { get; set; }

    }

    public enum Status
    {
        Pending,
        Success
    }
}
