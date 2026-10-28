# The planted page of TestDocsSamplesCheckNamesEachPlantedSample

A sample that holds:

<!-- source: src/go2cs/internal/repoguard/testdata/docsamples-planted/source.cs:4-6 -->
```csharp
static int One() {
    return 1;
}
```

<!-- source: src/go2cs/internal/repoguard/testdata/docsamples-planted/source.cs:4-6 -->
```csharp
static int One() {
    return 7;
}
```

<!-- source: src/go2cs/internal/repoguard/testdata/docsamples-planted/source.cs:4, 8 -->
```csharp
static int One() {

static int Two() {
```

<!-- source: the planted generator's output, abridged -->
```csharp
var prose = 1;
```

```csharp
var unlabelled = 1;
```

<!-- source (not checked): the planted generator's output -->
```csharp
var generated = 1;
```

<!-- illustration: written for this page -->
```csharp
var labelled = 1;
```
