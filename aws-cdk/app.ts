import * as cdk from 'aws-cdk-lib';

import { Stack } from './stack';

class App extends cdk.App {
  constructor() {
    super();
    new Stack(this, 'Bounan-Bot', {});
  }
}

new App().synth();
