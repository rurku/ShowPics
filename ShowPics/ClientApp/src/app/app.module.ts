import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { MediaBrowserModule } from './media-browser/media-browser.module';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './navmenu/navmenu.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    MediaBrowserModule,
    RouterModule.forRoot([
      { path: '', redirectTo: 'browse', pathMatch: 'full' },
      { path: '**', redirectTo: 'browse' }
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
