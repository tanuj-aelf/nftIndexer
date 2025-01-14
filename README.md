# NFT Indexer

A blockchain indexer service built on AEFinder SDK for indexing NFT-related events on the AELF blockchain.

## Overview

This project is designed to index and track NFT-related events and transactions on the AELF blockchain, providing GraphQL endpoints for querying NFT transfer and other related data.

## Prerequisites

- .NET 8.0 or later
- AEFinder SDK
- GraphQL

## Project Structure

```
nftIndexer/
├── src/                    # Source code
│   └── nftIndexer/         # Main project code
├── test/                   # Test files
├── NuGet.Config           # NuGet configuration
└── nftIndexer.sln         # Solution file
```

## Features

- NFT Transfer event tracking
- GraphQL API for querying NFT data
- Integration with AEFinder SDK
- Automated event processing

## Getting Started

1. Clone the repository
2. Restore NuGet packages
3. Build the solution
4. Run the indexer

```bash
dotnet restore
dotnet build
```

## Configuration

Configure your indexer settings in the appropriate configuration files. Make sure to set up your AEFinder SDK configuration properly.