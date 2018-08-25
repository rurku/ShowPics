import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TreeModule } from 'angular-tree-component';

import { TreeComponent } from './tree/tree.component';
import { ThumbnailsComponent } from './thumbnails/thumbnails.component';
import { MediaBrowserComponent } from './media-browser.component';
import { RouterModule } from '@angular/router';

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild([
      { path: 'browse', component: MediaBrowserComponent }
    ]),
    TreeModule.forRoot()
  ],
  declarations: [TreeComponent, ThumbnailsComponent, MediaBrowserComponent],
  exports: []
})
export class MediaBrowserModule { }
