import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { TreeElement, TreeElementType } from './tree-element';

@Component({
  selector: 'app-tree',
  templateUrl: './tree.component.html',
  styleUrls: ['./tree.component.css']
})
export class TreeComponent implements OnInit {
  selectedElement: TreeElement;
  elements: Array<TreeElement> = [new TreeElement(TreeElementType.File, 'file1'), new TreeElement(TreeElementType.Folder, 'folder1')];
  @Output() onSelected = new EventEmitter<TreeElement>();

  constructor() { }

  ngOnInit() {
  }

  onSelect(element: TreeElement) {
    this.selectedElement = element;
    this.onSelected.emit(element);
  }
}
