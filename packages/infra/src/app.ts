import { App as AwsApp } from 'aws-cdk-lib';

import { ToFillCdkStack } from './stack';

class App extends AwsApp {
  constructor() {
    super();
    new ToFillCdkStack(this, 'Bounan-ToFill', {});
  }
}

new App().synth();
