# JsonSizer

`jsonsizer` is a .NET CLI tool that shows what is taking up the space in large JSON documents.

## Install

```
dotnet tool install --global jsonsizer
```

## Usage

```
jsonsizer path/to/your/large.json
```

Gives size in bytes for each path like this:

```
@root 6
@root;first_name 26
@root;last_name 27
...
@root;phone_numbers;[i];number 70
@root;children 27
@root;children;[i] 53
@root;spouse 21
```

Which can be forwarded into [Brendan Gregg's FlameGraph tool](https://github.com/brendangregg/FlameGraph) to get a nice visuzalization:

```
jsonsizer path/to/your/large.json | ./flamegraph.pl > flamegraph.svg
```
