# ElectionGuard

ElectionGuard is an open source software development kit (SDK) that makes voting more secure, transparent and accessible. The ElectionGuard SDK leverages homomorphic encryption to ensure that votes recorded by electronic systems of any type remain encrypted, secure, and secret. Meanwhile, ElectionGuard also allows verifiable and accurate tallying of ballots by any 3rd party organization without compromising secrecy or security.

Learn More in the [ElectionGuard Repository](https://github.com/microsoft/electionguard)

## CLI

This project is a command line utility that includes various functions to generate test data and interact with election guard. It is able to create an encryption package that would be used with an encryption device, it can simulate an encryption device by encrypting ballots, it can generate ballots from a manifest, and it can verify an election record.

## Getting Started

```bash
# from the project root
make build-cli

# test the command line tool
cd /apps/electionguard-cli
dotnet run encrypt -c ./path/to/context.json -m ./path/to/manifest.json -b ./path/to/plaintext-ballots -d ./path/to/device.json -o ./path/to/results

# pack the tool for local use
dotnet pack

# add the tool for global use
dotnet tool install --global --add-source ./nupkg electionguard

```

## Commands

The CLI Tool includes a number of commands for working with electionguard data.

### Create Election

Create an election package to be used by an encryption device.

### Encrypt Ballot

Encrypt Ballots using a context.

### Generate Ballots

Generate plaintext ballots from a manifest and context, and encrypt them.

### Verify Election Record

Verify an election record.