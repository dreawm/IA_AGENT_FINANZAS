import { Component } from '@angular/core';
import { ChatTutorPage } from './chat/chat-tutor.page';

@Component({
  selector: 'app-root',
  imports: [ChatTutorPage],
  template: '<app-chat-tutor />',
})
export class App {}
