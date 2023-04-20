import { Component, OnInit } from '@angular/core';
import { ElectionguardService } from './electionguard.service';
import { Logger } from './logger.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  private _elapsed: number = 0;
  private _timer: any;

  constructor(private service: ElectionguardService, private logger: Logger) {
    this.logger.log('AppComponent constructor');
  }
  async ngOnInit(): Promise<void> {
    await this.service.testAddNumbers(2, 3);

    // configure the service with the election config data
    await this.service.configure();

    // get a test ballot
    const ballot = await this.service.getTestBallot();

    // encrypt the ballot
    this.logger.log(`encrypting ballot`);
    const encryptedBallot = await this.service.encryptBallot(ballot);
    this.logger.log(`encrypted ballot: ${encryptedBallot.toJson(true)}`);

    // get the nonce and timestamp

    // reencrypt the ballot

    // verify the code matches
  }
  title = 'angular-demo';
}
