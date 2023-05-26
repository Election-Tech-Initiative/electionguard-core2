import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { ElectionguardService } from './electionguard.service';
import { Logger } from './logger.service';

@NgModule({
  declarations: [AppComponent],
  imports: [BrowserModule],
  providers: [ElectionguardService, Logger],
  bootstrap: [AppComponent],
})
export class AppModule {}
