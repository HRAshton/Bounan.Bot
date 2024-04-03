using Amazon.CDK;
using Bounan.Downloader.AwsCdk;

var app = new App();
_ = new BotCdkStack(app, "Bounan-Bot", new StackProps());
app.Synth();