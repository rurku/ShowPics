import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TreeComponent } from './tree.component';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FileService } from '../file.service';

describe('TreeComponent', () => {
  let component: TreeComponent;
  let fixture: ComponentFixture<TreeComponent>;

  beforeEach(async(() => {
    const fileServiceSpy = jasmine.createSpy('FileService');
    TestBed.configureTestingModule({
      declarations: [TreeComponent],
      providers: [
        { provide: FileService, useValue: fileServiceSpy }
      ],
      schemas: [ NO_ERRORS_SCHEMA ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
