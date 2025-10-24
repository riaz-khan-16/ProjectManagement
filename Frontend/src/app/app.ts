import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './app.component.html'
})
export class App {
  username = '';
  greeting = '';
  
  sayHello() {
    this.greeting = `Hello Riaz, ${this.username}!  Angular!`;
  }
}
