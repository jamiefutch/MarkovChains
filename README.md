# MarkovChains

A C# library for working with Markov Chains, featuring helpful extension methods.

## Features

- Extension methods for easier string output with timestamps.
- Designed for extensibility and integration into larger Markov Chain projects.

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later

### Building

Clone the repository and build the solution:

```sh
dotnet build
```
## Usage

## MarkovChainNGram Class

The MarkovChainNGram class implements a Markov chain using n-grams for text generation and analysis.
Implements IMarkovChain.

### Initialization:

Create an instance by specifying the n-gram order and the initial dictionary capacity:
```
var markov = new MarkovChainNGram(order: 2, chainCapacity: 10000);
```

### Training:

Train the chain with input text. The text is cleaned of punctuation and split by whitespace.

```
markov.Train("your input text here");
```

### Text Generation:

Generate text with an optional starting n-gram and a maximum word count.

```
string output = markov.Generate(start: null, maxWords: 50);
```

### Persistence:

Save or load the chain to/from a JSON file.

```
markov.SaveToFile("chain.json");
markov.LoadFromFile("chain.json");
```

### Memory Management:

Use TrimChain() to reduce dictionary memory usage, and Dispose() to release resources.

```
markov.TrimChain();
markov.Dispose();
```

### Exceptions:

Throws ArgumentException if order or capacity is less than 1.
Throws InvalidOperationException if generating from an empty chain.
Throws FileNotFoundException if loading from a missing file.

## MarkovChainSimd Class

The MarkovChainNGram class is a Simd implementation of a Markov chain using n-grams for text generation and analysis.
Implements IMarkovChain.

### Initialization:

Create an instance by specifying the n-gram order and the initial dictionary capacity:
```
var markov = new MarkovChainNGram(order: 2, chainCapacity: 10000);
```

### Training:

Train the chain with input text. The text is cleaned of punctuation and split by whitespace.

```
markov.Train("your input text here");
```

### Text Generation:

Generate text with an optional starting n-gram and a maximum word count.

```
string output = markov.Generate(start: null, maxWords: 50);
```

### Persistence:

Save or load the chain to/from a JSON file.

```
markov.SaveToFile("chain.json");
markov.LoadFromFile("chain.json");
```

### Memory Management:

Use TrimChain() to reduce dictionary memory usage, and Dispose() to release resources.

```
markov.TrimChain();
markov.Dispose();
```

### Exceptions:

Throws ArgumentException if order or capacity is less than 1.
Throws InvalidOperationException if generating from an empty chain.
Throws FileNotFoundException if loading from a missing file.

## MarkovChainSqlite

`MarkovChainSqlite` is a C# Markov chain text generator that uses SQLite for persistent, efficient storage and retrieval of n-gram statistics. It supports training, text generation, and pruning of rare n-grams.

### Features

- Trains on text using n-grams of configurable order
- Stores n-gram statistics in a SQLite database (file or in-memory)
- Generates text based on learned patterns
- Prunes rare n-grams to optimize database size
- Fast, persistent, and easy to use

### Usage

```csharp
using MarkovChains;

// Create or open a Markov chain database
using var chain = new MarkovChainSqlite("mydb.sqlite", order: 2);

// Train the chain with text
chain.Train("This is a sample sentence.");
chain.Train(new[] { "Another example.", "More data improves results." });

// Generate text
string output = chain.Generate(maxWords: 20);

// Prune rare n-grams
chain.PruneChain(minCount: 2);

// Close the chain (optional, handled by Dispose)
chain.Close();
```

### Constructor

```csharp
public MarkovChainSqlite(string dbPath, int order, bool loadIntoMemory = false, int cacheSize = 1_000_000)
```

- `dbPath`: Path to the SQLite database file.
- `order`: N-gram size (e.g., 2 for bigrams).
- `loadIntoMemory`: If true, loads the database into memory for faster access (default: false).
- `cacheSize`: SQLite cache size (default: 1,000,000).

### Using Pre-Generated Chains
Specify the exisiting chain in the constructor and the Generate(...).



## License

```
MIT License

Copyright (c) 2025 Jamie Futch

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```