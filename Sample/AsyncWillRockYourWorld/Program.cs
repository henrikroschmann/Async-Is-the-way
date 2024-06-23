// See https://aka.ms/new-console-template for more information
using AsyncWillRockYourWorld;

Console.WriteLine("Example of Async Await evolution");

SampleNet1Async.Execute();

SampleNet2Async.Execute();

var net5 = new SampleNet5();
await net5.ExecuteAsync();