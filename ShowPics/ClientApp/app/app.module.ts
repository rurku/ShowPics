import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
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
    CommonModule,
    HttpModule,
    FormsModule,
    MediaBrowserModule,
    RouterModule.forRoot([
      { path: '', redirectTo: 'browse', pathMatch: 'full' },
      { path: '**', redirectTo: 'browse' }
    ])
  ]
})
export class AppModuleShared {
}
