import { Component, OnInit } from '@angular/core';
import { TreeElement, TreeElementType } from './tree/tree-element';

@Component({
  selector: 'app-media-browser',
  templateUrl: './media-browser.component.html',
  styleUrls: ['./media-browser.component.css']
})
export class MediaBrowserComponent implements OnInit {

  selectedTreeElement: TreeElement;
  treeElementType = TreeElementType;

  constructor() { }

  ngOnInit() {
  }

  onTreeElementSelected(element: TreeElement) {
    this.selectedTreeElement = element;
  }
}
