import { Component, OnInit } from '@angular/core';
import { TreeElement } from './tree/tree-element';

@Component({
  selector: 'app-media-browser',
  templateUrl: './media-browser.component.html',
  styleUrls: ['./media-browser.component.css']
})
export class MediaBrowserComponent implements OnInit {

  selectedItem: TreeElement;

  constructor() { }

  ngOnInit() {
  }
}
